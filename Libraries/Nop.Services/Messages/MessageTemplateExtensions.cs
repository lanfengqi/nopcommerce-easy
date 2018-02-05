using System.Collections.Generic;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Represents message template  extensions
    /// </summary>
    public static class MessageTemplateExtensions
    {
        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public static IEnumerable<string> GetTokenGroups(this MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            switch (messageTemplate.Name)
            {
                case MessageTemplateSystemNames.CustomerRegisteredNotification:
                case MessageTemplateSystemNames.CustomerWelcomeMessage:
                case MessageTemplateSystemNames.CustomerEmailValidationMessage:
                case MessageTemplateSystemNames.CustomerEmailRevalidationMessage:
                case MessageTemplateSystemNames.CustomerPasswordRecoveryMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens };


                case MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage:
                case MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens };

                case MessageTemplateSystemNames.PrivateMessageNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.CustomerTokens };

                case MessageTemplateSystemNames.NewsCommentNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.CustomerTokens };

                case MessageTemplateSystemNames.BackInStockNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ProductBackInStockTokens };

                case MessageTemplateSystemNames.ContactUsMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactUs };


                default:
                    return new string[] { };
            }
        }
    }
}