using System;
using System.Collections.Generic;
//using Newtonsoft.Json;
using System.Linq;

namespace NazoBrain
{
    [Serializable]
	public class Word
	{
		public string MyText { get; set; }

		public DateTime TimeStamp { get; set; }

		private List<WordCandidate> _candidates;

		public List<WordCandidate> Candidates
		{
			get
			{
				for (var i = 0; i < _candidates.Count; i++)
					if ((DateTime.UtcNow - _candidates[i].RegisteredTime).TotalHours > NazoBrainModel.WORD_LIFE_SPAN)
						_candidates.Remove(_candidates[i]);
				return _candidates;
			}
			set
			{
				_candidates = value;
			}
		}


		public WordCandidate Add(string c) => Add(c, DateTime.UtcNow);

		public WordCandidate Add(string c, DateTime time)
		{
            WordCandidate wc;
			if ((wc = Candidates.FirstOrDefault(ca => ca.MyText == c) as WordCandidate) != null)
			{
				Candidates.Add(wc);
				wc.RegisteredTime = time;
				return wc;
			}
			Candidates.Add(wc = new WordCandidate(c, new List<WordCandidateChild>(), time));
			return wc;
		}


		public Word(string c) : this(c, new List<WordCandidate>(), DateTime.UtcNow) { }

		//[JsonConstructor]
		public Word(string myChar, List<WordCandidate> candidates, DateTime datetime = default(DateTime))
		{
			MyText = myChar;
			_candidates = candidates ?? throw new ArgumentNullException("candidates");

			TimeStamp = datetime.Equals(default(DateTime)) ? DateTime.UtcNow : datetime;
		}
	}


}