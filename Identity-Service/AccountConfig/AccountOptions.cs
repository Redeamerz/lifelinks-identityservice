using System;

namespace Identity_Service.AccountConfig
{
	public static class AccountOptions
	{
		private static readonly bool allowLocalLogin = true;
		private static readonly bool allowRememberLogin = true;
		private static readonly TimeSpan rememberMeLoginDuration = TimeSpan.FromDays(30);

		private static readonly bool showLogoutPrompt = true;
		private static readonly bool automaticRedirectAfterSignOut = true;

		private static readonly string invalidCredentialsErrorMessage = "Invalid username or password";

		public static bool AllowLocalLogin => allowLocalLogin;

		public static bool AllowRememberLogin => allowRememberLogin;

		public static TimeSpan RememberMeLoginDuration => rememberMeLoginDuration;

		public static bool ShowLogoutPrompt => showLogoutPrompt;

		public static bool AutomaticRedirectAfterSignOut => automaticRedirectAfterSignOut;

		public static string InvalidCredentialsErrorMessage => invalidCredentialsErrorMessage;
	}
}