using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Buffers.Binary;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WhosHereServer.Data;
using WhosHereServer.Models;
using WhosHereServer.Services;

namespace WhosHereServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly IChatRelayService chatRelayService;

        public ChatController(
            ILogger<ChatController> logger, 
            UserManager<AppUser> userManager, 
            IChatRelayService chatRelayService)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.chatRelayService = chatRelayService;
        }

        private event RelayMessageReceivedEventHandler? relayMessageReceived;
        private WebSocket? webSocket;
        private AppUser? currentUser;

        [HttpGet("ws")]
        [Authorize]
        public async Task<IActionResult> Hello() 
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                currentUser = await userManager.GetUserAsync(User);
                relayMessageReceived += ChatController_RelayMessageReceived;
                chatRelayService.RegisterUser(currentUser.Id, relayMessageReceived);
                await HandleChatClient();
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        private async void ChatController_RelayMessageReceived(object sender, RelayMessageReceivedEventArgs e)
        {
            string json = JsonSerializer.Serialize(new ReceiveChatMessageDto
            {
                SenderId = e.ChatMessage.SenderId,
                Message = e.ChatMessage.Message,
                SendTime = e.ChatMessage.SendTime,
            });
            await webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task HandleChatClient()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    var buffer = new byte[4096];
                    var bufSegment = new ArraySegment<byte>(buffer);
                    var receiveResult = await webSocket.ReceiveAsync(bufSegment, CancellationToken.None);

                    while (!receiveResult.CloseStatus.HasValue)
                    {
                        ms.Write(bufSegment.Array!, bufSegment.Offset, receiveResult.Count);
                        if (receiveResult.EndOfMessage && ms.Length > 0)
                        {
                            StreamReader reader = new StreamReader(ms);
                            string str = Encoding.UTF8.GetString(ms.ToArray());
                            await HandleClientRequest(str);
                            ms.Seek(0, SeekOrigin.Begin);
                        }
                        //TODO: limit buffer size

                        bufSegment = new ArraySegment<byte>(buffer);
                        receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    }

                    await webSocket.CloseAsync(
                        receiveResult.CloseStatus.Value,
                        receiveResult.CloseStatusDescription,
                        CancellationToken.None);
                    chatRelayService.UnregisterUser(currentUser!.Id);
                }
                catch (WebSocketException ex)
                {
                    logger.LogInformation(ex.Message);
                    chatRelayService.UnregisterUser(currentUser!.Id);
                }
            }
        }

        private async Task HandleClientRequest(string requestMessage)
        {
            SendChatMessageDto? sendDto = JsonSerializer.Deserialize<SendChatMessageDto>(requestMessage);
            if (sendDto == null)
            {
                logger.LogWarning($"Invalid request from user: {currentUser!.Email}");
            }
            AppUser? receiver = await userManager.FindByIdAsync(sendDto!.ReceiverId);
            if (receiver != null)
            {
                chatRelayService.SendChatMessage(new UserChatMessage
                {
                    Message = sendDto.Message,
                    ReceiverId = receiver.Id,
                    SenderId = currentUser!.Id,
                    SendTime = DateTime.Now
                });
            }
            else
            {
                //TODO: send failed response
            }
        }
    }
}
