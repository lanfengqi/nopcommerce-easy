namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// System customer attribute names
    /// </summary>
    public static partial class SystemCustomerAttributeNames
    {
        //Form fields
        /// <summary>
        /// FirstName
        /// </summary>
        public static string FirstName { get { return "FirstName"; } }
        /// <summary>
        /// LastName
        /// </summary>
        public static string LastName { get { return "LastName"; } }
        
        /// <summary>
        /// Phone
        /// </summary>
        public static string Phone { get { return "Phone"; } }

        /// <summary>
        /// TimeZoneId
        /// </summary>
        public static string TimeZoneId { get { return "TimeZoneId"; } }
        /// <summary>
        /// CustomCustomerAttributes
        /// </summary>
        public static string CustomCustomerAttributes { get { return "CustomCustomerAttributes"; } }

        //Other attributes
        /// <summary>
        /// DiscountCouponCode
        /// </summary>
        public static string DiscountCouponCode { get { return "DiscountCouponCode"; } }
        /// <summary>
        /// GiftCardCouponCodes
        /// </summary>
        public static string GiftCardCouponCodes { get { return "GiftCardCouponCodes"; } }
        /// <summary>
        /// AvatarPictureId
        /// </summary>
        public static string AvatarPictureId { get { return "AvatarPictureId"; } }

        /// <summary>
        /// PasswordRecoveryToken
        /// </summary>
        public static string PasswordRecoveryToken { get { return "PasswordRecoveryToken"; } }
        /// <summary>
        /// PasswordRecoveryTokenDateGenerated
        /// </summary>
        public static string PasswordRecoveryTokenDateGenerated { get { return "PasswordRecoveryTokenDateGenerated"; } }
        /// <summary>
        /// AccountActivationToken
        /// </summary>
        public static string AccountActivationToken { get { return "AccountActivationToken"; } }
        /// <summary>
        /// EmailRevalidationToken
        /// </summary>
        public static string EmailRevalidationToken { get { return "EmailRevalidationToken"; } }
        /// <summary>
        /// LastVisitedPage
        /// </summary>
        public static string LastVisitedPage { get { return "LastVisitedPage"; } }
        /// <summary>
        /// ImpersonatedCustomerId
        /// </summary>
        public static string ImpersonatedCustomerId { get { return "ImpersonatedCustomerId"; } }
        /// <summary>
        /// AdminAreaStoreScopeConfiguration
        /// </summary>
        public static string AdminAreaStoreScopeConfiguration { get { return "AdminAreaStoreScopeConfiguration"; } }

        /// <summary>
        /// LanguageId
        /// </summary>
        public static string LanguageId { get { return "LanguageId"; } }
        /// <summary>
        /// LanguageAutomaticallyDetected
        /// </summary>
        public static string LanguageAutomaticallyDetected { get { return "LanguageAutomaticallyDetected"; } }
        
        /// <summary>
        /// WorkingThemeName
        /// </summary>
        public static string WorkingThemeName { get { return "WorkingThemeName"; } }

        /// <summary>
        /// EuCookieLawAccepted
        /// </summary>
        public static string EuCookieLawAccepted { get { return "EuCookieLaw.Accepted"; } }
    }
}