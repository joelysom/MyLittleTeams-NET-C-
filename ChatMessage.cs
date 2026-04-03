using System;

namespace MeuApp
{
    public class ChatMessage
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public string StickerAsset { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsOwn { get; set; }

        public bool IsSticker => string.Equals(MessageType, "sticker", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(StickerAsset);

        public string ConversationPreview => IsSticker
            ? (!string.IsNullOrWhiteSpace(Content) ? Content : "Figurinha enviada")
            : Content;
    }
}