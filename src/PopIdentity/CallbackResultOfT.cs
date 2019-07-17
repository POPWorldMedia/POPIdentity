namespace PopIdentity
{
	public class CallbackResult<T> where T : class
	{
		public T ResultData { get; set; }
		public bool IsSuccessful { get; set; }
		public string Message { get; set; }
	}
}