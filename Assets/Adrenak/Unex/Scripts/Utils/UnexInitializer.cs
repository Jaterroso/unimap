namespace Adrenak.Unex {
	public static class UnexInitializer {
		public static void Run() {
			Dispatcher.Create();
			Runner.Init();
		}
	}
}
