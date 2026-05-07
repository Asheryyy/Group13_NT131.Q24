namespace BEapp.Interface
{
	public class ISystemCreate
	{
		public interface ISystemState
		{
			bool IsManualMode { get; set; }
			bool IsPumpOn { get; set; } // ← Thêm cái này
			DateTime? LastManualCommandTime { get; set; }
		}

		public class SystemState : ISystemState
		{
			public bool IsManualMode { get; set; } = false;
			public bool IsPumpOn { get; set; } = false; // ← Thêm cái này
			public DateTime? LastManualCommandTime { get; set; } = null;
		}
	}
}