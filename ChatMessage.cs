using System;

namespace MeuApp
{
    public class ChatMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public string StickerAsset { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsOwn { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsSticker => string.Equals(MessageType, "sticker", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(StickerAsset);

        public bool IsText => string.Equals(MessageType, "text", StringComparison.OrdinalIgnoreCase);

        public string DeletedDisplayText => $"'{(string.IsNullOrWhiteSpace(SenderName) ? "Usuário" : SenderName)}' apagou essa mensagem (X)";

        public string ConversationPreview => IsDeleted
            ? DeletedDisplayText
            : IsSticker
                ? (!string.IsNullOrWhiteSpace(Content) ? Content : "Figurinha enviada")
                : Content;
    }
}