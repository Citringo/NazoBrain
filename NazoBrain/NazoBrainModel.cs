using NMeCab;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NazoBrain
{

	public interface IPost
	{
		string Message { get; }
		string Name { get; }
	}

	public class UserPost : IPost
	{
		public string Message { get; }
		public string Name { get; }
		public UserPost(string name, string mes)
		{
			Message = mes ?? "";
			Name = name ?? "NO NAME";
		}
	}

	public class BotPost : IPost
	{
		public string Message { get; }
		public string Name => "NazoBrain";
		public BotPost(string mes)
		{
			Message = mes;
		}
	}

	public class NazoBrainModel : BindableBase
    {
        Dictionary<string, Word> wordBrain = new Dictionary<string, Word>();
        
		List<int> lengthBrain = new List<int>();

		private string _userName;
		public string UserName
		{
			get => _userName;
			set => SetProperty(ref _userName, value);
		}

		public Dictionary<string, Word> WordBrain
        {
            get 
            {
                return wordBrain;
            }
        }

		public ObservableCollection<IPost> Posts { get; }

		/// <summary>
		/// 記憶の時間単位の寿命。
		/// </summary>
		public static readonly int WORD_LIFE_SPAN = 4;

        private MeCabTagger _tagger;

		public void Post(string mes)
		{
			if (mes == null) return;
			string s;
			Posts.Add(new UserPost(UserName, mes));
			Posts.Add(new BotPost(s = Say(mes)));
			Learn(mes);
			Learn(s);
		}

        public NazoBrainModel()
        {
			_tagger = MeCabTagger.Create();
			_tagger.OutPutFormatType = "wakati";
			Posts = new ObservableCollection<IPost>();
			_userName = "あなた";
        }

        public void Learn(string text) 
        {
            Learn(text, DateTime.UtcNow);
        }

		UnicodeBlock prevBlock;

		/// <summary>
		/// 単語を分割する文字を登録します。
		/// </summary>
		public readonly List<string> Divider = new List<string>
		{
			"て", "に", "を", "は", "が", "で", "と", "の", "や", "へ", "も",
			"こ", "そ", "あ", "ど", "、", "。", "，", "．", "！", "？"
		};

		/// <summary>
		/// 文を単語ごとに分割します。
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		string[] Split(string text)
		{
			if (text.Length == 0)
				return new string[0];

			//if (config.UseMecab)
			return _tagger.Parse(text).Replace("\r", "").Replace("\n", "").Split(' ').Where(s => !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s)).ToArray();
			
			var list = new List<string>();
			var buf = "";
			for (var i = 0; i < text.Length; i++)
			{
				UnicodeBlock block = text[i].GetBlock();
				if (Divider.Contains(text[i].ToString()) ||
					char.IsSeparator(text[i]) || 
					char.IsSymbol(text[i]))
				{
					if (!string.IsNullOrEmpty(buf) && !string.IsNullOrWhiteSpace(buf))
						list.Add(buf);
						list.Add(text[i].ToString());
					buf = "";
					continue;
				}

				if (block != prevBlock)
				{
					if (!string.IsNullOrEmpty(buf) && !string.IsNullOrWhiteSpace(buf))
						list.Add(buf);
					buf = "";
				}

				buf += text[i];
				prevBlock = block;
			}
			if (!string.IsNullOrEmpty(buf))
				list.Add(buf);
			return list.ToArray();
		}

		string[] prevList;

        /// <summary>
        /// 文を学習します。
        /// </summary>
        /// <param name="texts"></param>
        /// <param name="timeStamp"></param>
		public void Learn(string texts, DateTime timeStamp)
		{
			if (texts.Length == 0)
				return;
            texts = Regex.Replace(texts, "<[@#].+?>", "");
            // 改行を統一
            texts = texts.Replace("\r\n", "\n").Replace('\r', '\n');
            // 改行で区切って繰り返し学習
            foreach (var text in texts.Split('\n'))
            {
                string[] list = Split(text);
				if (list.Length == 0)
					continue;
				string now = "", next, nenext;
				
				if (prevList != null && prevList.Length > 0)
				{
					// 前の発言の最後の単語を取り出す
						var prevWord = prevList.LastOrDefault();
					// その単語が記憶されてなかったら追加しておく

					if (!wordBrain.ContainsKey(prevWord))
						wordBrain.Add(prevWord, new Word(prevWord));
					// その単語の\0候補の子候補リストに、今の発言の最初の単語を加える
					wordBrain[prevWord].Add("\0", timeStamp).Add(list[0], timeStamp);

					if (!wordBrain.ContainsKey("\0"))
						wordBrain.Add("\0", new Word("\0"));
					WordCandidate wc = wordBrain["\0"].Add(list[0], timeStamp);
					if (list.Length > 1)
						wc.Add(list[1], timeStamp);

				}

                for (var i = 0; i < list.Length; i++)
                {
                    now = list[i];
					next = i < list.Length - 1 ? list[i + 1] : "\0";
					nenext = i < list.Length - 2 ? list[i + 2] : "\0";
					if (!wordBrain.ContainsKey(now))
                        wordBrain[now] = new Word(now);
					WordCandidate wc = wordBrain[now].Add(next, timeStamp);
					if (!Null(nenext))
						wc.Add(nenext, timeStamp);
                }
                lengthBrain.Add(text.Length);
				prevList = list;
            }
		}

		Random r = new Random();

		public void Clear()
		{
			wordBrain.Clear();
		}

		string Chain(string word)
		{
			var buf = new StringBuilder();
			//buf.Append(word);
			Word w = wordBrain[word];
			Word center;
			while (!Null(word) && w != null)
			{
				WordCandidate mychild = w.Candidates.Random();
				if (Null(mychild?.MyText))
					break;
				buf.Append(mychild.MyText);
				WordCandidateChild childschild = mychild.Candidates.Random();

				if (Null(childschild?.MyText))
					break;
				//buf.Append(childschild.MyText);
				center = wordBrain[mychild.MyText];
                var a = center.Candidates.Find(wc => wc.MyText == childschild.MyText);
                var b = center.Candidates.Random();
				word = (a?.MyText) ?? (b?.MyText);
                if (word == null)
                    continue;
				buf.Append(word);
				w = wordBrain[word];
			}
			return buf.ToString();
		}

        bool Null(string str)
        {
            return string.IsNullOrEmpty(str) || str == "\0";
        }

		string Say(string text)
		{
			if (wordBrain.Count == 0)
				return "";
			return Chain(NullCharFilter(FindChainCandidate(text) ?? ""));
		}

		private string NullCharFilter(string v)
		{
			// その単語の子要素にnull文字がなければ入力そのままで返す
            WordCandidate wc;
			if (!((wc = wordBrain[v].Candidates.FirstOrDefault(c => c.MyText == "\0")) as WordCandidate != null))
				return v;
            WordCandidateChild wcc;
			if (!((wcc = wc.Candidates.Random() as WordCandidateChild) != null))
				return v;
			if (!WordBrain.ContainsKey(wcc.MyText))
				return v;
			return wcc.MyText;
		}

		private string FindChainCandidate(string text)
		{
			string[] arglist = Split(text).Reverse().Take(3).Reverse().ToArray();
			if (arglist.Length >= 1 && wordBrain.ContainsKey(arglist[0]))
			{
				Word s1 = wordBrain[arglist[0]];
				if (arglist.Length >= 2)
					if (s1.Candidates.FirstOrDefault(w => w.MyText == arglist[1]) is WordCandidate wc)
						if (arglist.Length >= 3)
							if (wc.Candidates.FirstOrDefault(w => w.MyText == arglist[2]) is WordCandidate wcc)
								return arglist[2];
							else
								return wc.Candidates.Count > 0 ? wc.Candidates.Random().MyText : wordBrain.Random().Key;
						else
							return arglist[1];
					else
						return s1.Candidates.Count > 0 ? s1.Candidates.Random().MyText : wordBrain.Random().Key;
				else
					return arglist[0];
			}
			else
				return wordBrain.Random().Key;
		}
    }

}
