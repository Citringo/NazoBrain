using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NazoBrain
{
	/// <summary>
	/// 
	/// </summary>
	class TalkWindowViewModel : BindableBase
    {
		private NazoBrainModel nbm;
		private string post, learn;

		public ObservableCollection<IPost> Posts => nbm.Posts;

		public string PostText
		{
			get => post;
			set
			{
				SetProperty(ref post, value);
				PostCommand.RaiseCanExecuteChanged();
			}
		}

		public string LearnText
		{
			get => learn;
			set
			{
				SetProperty(ref learn, value);
				LearnCommand.RaiseCanExecuteChanged();
			}
		}


		public TalkWindowViewModel()
        {
			nbm = new NazoBrainModel();
			PostCommand = new DelegateCommand
			{
				CanExecuteHandler = (o) => nbm != null && !string.IsNullOrWhiteSpace(PostText),
				ExecuteHandler = (o) =>
				{
					nbm.Post(PostText.ToString());
					PostText = "";
				}
			};

			LearnCommand = new DelegateCommand
			{
				CanExecuteHandler = (o) => nbm != null && !string.IsNullOrWhiteSpace(LearnText),
				ExecuteHandler = (o) =>
				{
					nbm.Learn(LearnText.ToString());
					LearnText = "";
				}
			};
		}


		public DelegateCommand PostCommand { get; }
		public DelegateCommand LearnCommand { get; }

    }

}
