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
        public string AttachmentFileName { get; set; } = string.Empty;
        public string AttachmentContentType { get; set; } = string.Empty;
        public string AttachmentStoragePath { get; set; } = string.Empty;
        public long AttachmentSizeBytes { get; set; }
        public string AttachmentLocalPath { get; set; } = string.Empty;
        public string AttachmentPreviewDataUri { get; set; } = string.Empty;
        public string MediaGroupId { get; set; } = string.Empty;
        public int MediaGroupIndex { get; set; }
        public int MediaGroupCount { get; set; }
        public string LinkUrl { get; set; } = string.Empty;
        public string LinkTitle { get; set; } = string.Empty;
        public string LinkDescription { get; set; } = string.Empty;
        public string LinkImageUrl { get; set; } = string.Empty;
        public string LinkSiteName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsOwn { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsSticker => string.Equals(MessageType, "sticker", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(StickerAsset);

        public bool IsText => string.Equals(MessageType, "text", StringComparison.OrdinalIgnoreCase);

        public bool IsImageAttachment => string.Equals(MessageType, "image", StringComparison.OrdinalIgnoreCase);

        public bool IsVideoAttachment => string.Equals(MessageType, "video", StringComparison.OrdinalIgnoreCase);

        public bool IsAudioAttachment => string.Equals(MessageType, "audio", StringComparison.OrdinalIgnoreCase);

        public bool IsFileAttachment => string.Equals(MessageType, "file", StringComparison.OrdinalIgnoreCase);

        public bool HasAttachment => !IsDeleted
            && (IsImageAttachment || IsVideoAttachment || IsAudioAttachment || IsFileAttachment)
            && (!string.IsNullOrWhiteSpace(AttachmentStoragePath) || !string.IsNullOrWhiteSpace(AttachmentFileName));

        public bool IsMediaGroupItem => !IsDeleted
            && IsImageAttachment
            && !string.IsNullOrWhiteSpace(MediaGroupId)
            && MediaGroupCount > 1;

        public bool HasLinkPreview => !IsDeleted
            && !string.IsNullOrWhiteSpace(LinkUrl)
            && (!string.IsNullOrWhiteSpace(LinkTitle)
                || !string.IsNullOrWhiteSpace(LinkDescription)
                || !string.IsNullOrWhiteSpace(LinkImageUrl)
                || !string.IsNullOrWhiteSpace(LinkSiteName));

        public string AttachmentDisplayLabel => IsImageAttachment
            ? "Imagem"
            : IsVideoAttachment
                ? "Video"
                : IsAudioAttachment
                    ? "Audio"
                    : IsFileAttachment
                        ? "Arquivo"
                        : "Anexo";

        public string LinkDisplayHost
        {
            get
            {
                if (Uri.TryCreate(LinkUrl, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
                {
                    return uri.Host;
                }

                return string.IsNullOrWhiteSpace(LinkSiteName) ? string.Empty : LinkSiteName;
            }
        }

        public string DeletedDisplayText => $"'{(string.IsNullOrWhiteSpace(SenderName) ? "Usuário" : SenderName)}' apagou essa mensagem (X)";

        public string ConversationPreview => IsDeleted
            ? DeletedDisplayText
            : IsSticker
                ? (!string.IsNullOrWhiteSpace(Content) ? Content : "Figurinha enviada")
                : HasAttachment
                    ? (!string.IsNullOrWhiteSpace(Content)
                        ? Content
                        : $"{AttachmentDisplayLabel} • {(string.IsNullOrWhiteSpace(AttachmentFileName) ? "anexo" : AttachmentFileName)}")
                    : Content;
    }
}