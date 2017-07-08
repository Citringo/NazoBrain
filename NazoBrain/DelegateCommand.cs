using System;
using System.Windows.Input;

namespace NazoBrain
{
	public class DelegateCommand : ICommand
	{
		public Action<object> ExecuteHandler { get; set; }
		public Func<object, bool> CanExecuteHandler { get; set; }

		#region ICommand メンバー

		public bool CanExecute(object parameter)
		{
			var d = CanExecuteHandler;
			return d == null ? true : d(parameter);
		}

		public void Execute(object parameter) => ExecuteHandler?.Invoke(parameter);

		public event EventHandler CanExecuteChanged;

		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, null);

		#endregion
	}

}
