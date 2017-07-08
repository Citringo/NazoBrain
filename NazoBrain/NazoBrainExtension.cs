using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NazoBrain
{
	static class NazoBrainExtension
	{
		/// <summary>
		/// この文字が属している Unicode ブロックを返します。
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static UnicodeBlock GetBlock(this char c)
		{
			if (c <= '\u02af')
				return UnicodeBlock.Laten;
			if ('\u1d00' <= c && c <= '\u2bff')
                return UnicodeBlock.Symbol;
			if ('\u3040' <= c && c <= '\u309f')
                return UnicodeBlock.Hiragana;
			if ('\u30a0' <= c && c <= '\u30ff')
                return UnicodeBlock.Katakana;
			if ('\u3400' <= c && c <= '\u4dbf')
                return UnicodeBlock.Kanji;
			if ('\u4e00' <= c && c <= '\u9fff')
                return UnicodeBlock.Kanji;
			if ('\u3400' <= c && c <= '\u4dbf')
                return UnicodeBlock.Kanji;
			if ('\uf900' <= c && c <= '\ufaf0')
                return UnicodeBlock.Kanji;
			if ('\uff00' <= c && c <= '\uff60')
                return UnicodeBlock.ZenkakuLatin;
			if ('\uff61' <= c && c <= '\uff9f')
                return UnicodeBlock.HankakuKatakana;
            return UnicodeBlock.Other;
		}
		
	}

	public static class LinqExtension
	{
		private static Random _rand = new Random();
		public static T Random<T>(this IEnumerable<T> ie) => ie.Count() > 0 ? ie.ElementAt(_rand.Next(ie.Count())) : default(T);

		// http://stackoverflow.com/questions/5807128/an-extension-method-on-ienumerable-needed-for-shuffling
		// Thanks to LukeH
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(new Random());

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (rng == null) throw new ArgumentNullException(nameof(rng));

			return source.ShuffleIterator(rng);
		}

		private static IEnumerable<T> ShuffleIterator<T>(
			this IEnumerable<T> source, Random rng)
		{
			var buffer = source.ToList();
			for (var i = 0; i < buffer.Count; i++)
			{
				var j = rng.Next(i, buffer.Count);
				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}
	}

}