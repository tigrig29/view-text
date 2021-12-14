using System.Text.RegularExpressions;


/// <summary>
/// 画面描画用テキストクラス
/// </summary>
public class ViewText
{
    /// <summary>
    /// 半角文字の正規表現
    /// </summary>
    private const string HALF_WIDTH_CHAR_PATTERN = "[ -~｡-ﾟ]";
    private const string HALF_WIDTH_CHARS_PATTERN = "[ -~｡-ﾟ]+";

    /// <summary>
    /// 全角文字の正規表現（半角文字以外という表現）
    /// </summary>
    private const string FULL_WIDTH_CHAR_PATTERN = "[^ -~｡-ﾟ]";
    private const string FULL_WIDTH_CHARS_PATTERN = "[^ -~｡-ﾟ]+";

    public string text { get; private set; }

    /// <summary>
    /// 画面幅からはみ出る場合などの文字省略時に使用する記号
    /// </summary>
    /// <value></value>
    public string ellipsis { get; private set; }

    /// <param name="text">対象の文字列</param>
    /// <param name="ellipsis">画面幅からはみ出る場合などの文字省略時に使用する記号</param>
    /// <returns></returns>
    public ViewText(string text, string ellipsis)
    {
        this.text = text;
        this.ellipsis = ellipsis;
    }

    /// <param name="text">対象の文字列</param>
    /// <returns></returns>
    /// <remarks>ViewText.ellipsis = "…" （デフォルト値）となる</remarks>
    public ViewText(string text) : this(text, "…") { }

    /// <summary>
    /// 文字列の幅を取得する
    /// </summary>
    /// <returns>半角文字を 1, 全角文字を 2 とカウントし、対象文字列の合計カウントを返す</returns>
    public int GetWidth() => this.GetWidth(text);

    private int GetWidth(string value)
    {
        // 半角文字のみを抽出
        string halfWidthString = string.Empty;
        foreach (Match mc in Regex.Matches(value, HALF_WIDTH_CHARS_PATTERN))
        {
            halfWidthString += mc.ToString();
        }

        // 全角文字＝半角文字以外の文字を抽出
        string fullWidthString = Regex.Replace(value, HALF_WIDTH_CHARS_PATTERN, string.Empty);

        // 半角英数記号 = 1, その他 = 2 でカウント
        return halfWidthString.Length + fullWidthString.Length * 2;
    }

    /// <summary>
    /// 指定した文字幅で ViewText.text を切り取る
    /// </summary>
    /// <param name="width">表示させたい文字幅（半角文字を 1, 全角文字を 2 として文字幅をカウント）</param>
    /// <returns>
    /// <para>- 指定文字幅が text の文字幅以上ならば、そのまま text を返す。</para>
    /// <para>- 指定文字幅が text の文字幅より小さいならば、Text を切り取って末尾に省略記号（ViewText.Ellipsis）を付与して返す。</para>
    /// </returns>
    public string OverflowEllipsis(int width)
    {
        // 文字列の幅以上の幅を指定したら、文字列をそのまま返す
        if (GetWidth() <= width) return text;

        int ellipsisWidth = GetWidth(ellipsis);

        // 省略記号の幅未満を指定したら空を、省略記号の幅を指定したら省略記号を返す
        if (width < ellipsisWidth) return string.Empty;
        if (width == ellipsisWidth) return ellipsis;

        // 全て半角文字の場合 width = length なので、substring 切り取り ＋ 省略記号
        if (!Regex.Match(text, FULL_WIDTH_CHAR_PATTERN).Success) return text.Substring(0, width - ellipsisWidth) + ellipsis;

        // ここから全角文字が存在する場合

        // 1. 元の文字列のうち全角文字を適当な半角文字2桁(@@)に置き換える
        // 2. この状態では全て半角文字の文字列なので、上述の通り substring 切り取り
        // 3. 切り取った文字列のうち @@ を適当な全角文字に置換する
        // 4. 出来上がった文字列の Length で元の文字列を Substring すれば指定幅切り取りと同義になる

        const string dummyChar = "@";
        string dummyCharTwice = $"{dummyChar}{dummyChar}";

        // ※予めダミー文字に使用している記号を適当な記号に変換（元のテキストにダミー文字と同じ記号が存在すると、3. の段階でずれる可能性があるため）
        string text_EscapedDummyChar = text.Replace(dummyChar, "$");
        // 1. 全角文字をダミー文字（半角記号 * 2）に置換（例：あ -> @@）
        string text_OnlyHalfWidthChar = Regex.Replace(text_EscapedDummyChar, FULL_WIDTH_CHAR_PATTERN, dummyCharTwice);
        // 2. 指定された幅 - ellipsisWidth（省略記号が後で入る分マイナスしてる）で切り取る
        string text_OnlyHalfWidthChar_CutOut = text_OnlyHalfWidthChar.Substring(0, width - ellipsisWidth);
        // 3. ダミー文字を全角文字に戻す（2. の切り取りでちょうど @@ が奇数単位で切れてしまう場合があるため、@ を空置換しておく）
        string text_RestoreFullWidthChar_CutOut = text_OnlyHalfWidthChar_CutOut.Replace(dummyCharTwice, "あ").Replace(dummyChar, string.Empty);
        // 4.ダミー文字を全角文字に戻して長さを取得 → これが元の text に対して substring で切り取るべき長さ
        int targetLength = text_RestoreFullWidthChar_CutOut.Length;

        return text.Substring(0, targetLength) + ellipsis;
    }
}