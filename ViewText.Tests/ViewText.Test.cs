using System;
using Xunit;
using Xunit.Abstractions;

public class ViewText_Test
{
    private readonly ITestOutputHelper _output;
    public ViewText_Test(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("test")]
    [InlineData("!\"#$%&'()-=^~\\|")]
    [InlineData(",<.>/?_;+:*@`[{]}")]
    [InlineData("ｧｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾜｦﾝｮ")]
    public void GetWidth_InputsOnlyHalfWidthCharacters_ReturnsItsLength(string text)
    {
        ViewText viewText = new ViewText(text);

        int expected = text.Length;
        var actual = viewText.GetWidth();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ＴＥＳＴ")]
    [InlineData("＆＆！”｜＋＊１２９０")]
    [InlineData("アイワヲン")]
    [InlineData("あああゎ")]
    [InlineData("各種漢字")]
    public void GetWidth_InputsOnlyFullWidthCharacters_ReturnsTwiceLength(string text)
    {
        ViewText viewText = new ViewText(text);

        int expected = text.Length * 2;
        var actual = viewText.GetWidth();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OverflowEllipsis_InputsWidth1_ReturnsEmpty()
    {
        ViewText viewText = new ViewText("test");

        int width = 1;

        var expected = string.Empty;
        var actual = viewText.OverflowEllipsis(width);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("!\"#$%&'()-=^~\\|")]
    [InlineData(",<.>/?_;+:*@`[{]}")]
    [InlineData("ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿ")]
    [InlineData("あ")]
    [InlineData("あいうえお")]
    [InlineData("あいう@えおかきくけこ")]
    public void OverflowEllipsis_InputsItsWidth_ReturnsTheTextAsIs(string text)
    {
        ViewText viewText = new ViewText(text);

        int width = viewText.GetWidth();

        // 入力文字列の幅以上の値で OverflowEllipsis すると、入力文字列がそのまま返されるはず
        var expected = text;
        var actual = viewText.OverflowEllipsis(width);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("!\"#$%&'()-=^~\\|")]
    [InlineData(",<.>/?_;+:*@`[{]}")]
    [InlineData("ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿ")]
    [InlineData("あ")]
    [InlineData("あいうえお")]
    [InlineData("あいう@えおかきくけこ")]
    public void OverflowEllipsis_InputsBiggerWidth_ReturnsTheTextAsIs(string text)
    {
        ViewText viewText = new ViewText(text);

        // 1 ~ 10 を生成（念のためその値を標準出力）
        int random1To10 = new Random().Next(1, 10);
        _output.WriteLine(random1To10.ToString());

        // 「入力文字列の幅以上という数値を作成（入力文字列の幅 + 0 ~ 10）
        int width = viewText.GetWidth() + random1To10;

        // 入力文字列の幅以上の値で OverflowEllipsis すると、入力文字列がそのまま返されるはず
        var expected = text;
        var actual = viewText.OverflowEllipsis(width);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("test", 2, "…")]
    [InlineData("!\"#$%&'()-=^~\\|", 8, "!\"#$%&…")]
    [InlineData(",<.>/?_;+:*@`[{]}", 8, ",<.>/?…")]
    [InlineData("ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿ", 14, "ｱｲｳｴｵｶｷｸｹｺｻｼ…")]
    [InlineData("あいうえお", 8, "あいう…")]
    [InlineData("佐藤加藤伊藤鈴木ホゲフガピヨ", 20, "佐藤加藤伊藤鈴木ホ…")]
    [InlineData("あいう@えお", 7, "あい…")]
    [InlineData("あいう@えお", 8, "あいう…")]
    [InlineData("あいう@えお", 9, "あいう@…")]
    [InlineData("あいう@えお", 10, "あいう@…")]
    [InlineData("あいう@えおか", 11, "あいう@え…")]
    [InlineData("@あいうえお@", 11, "@あいうえ…")]
    public void OverflowEllipsis_InputsSmallerWidth_ReturnsTheTextWithEllipsis(string text, int width, string expected)
    {
        ViewText viewText = new ViewText(text);

        var actual = viewText.OverflowEllipsis(width);
        Assert.Equal(expected, actual);
    }
}