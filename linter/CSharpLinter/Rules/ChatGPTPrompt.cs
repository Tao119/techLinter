using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace CSharpLinter
{
    public static class ChatGptFeedbackFetcher
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string? apiUrl = "https://techlinter-server.onrender.com/analyze";

        public static async Task FetchFeedbackAndAddToIssues(
            SyntaxTree tree,
            List<Issue> issues,
            int ur_id
        )
        {
            if (apiUrl == null)
            {
                return;
            }

            string code = AddLineNumbers(tree.ToString());

            var response = await SendCodeToExternalApi(code, ur_id);
            if (response != null)
            {
                issues.AddRange(response);
            }
        }

        private static async Task<List<Issue>> SendCodeToExternalApi(string code, int ur_id)
        {
            string prompt =
                $@"""
以下にcsで書かれたコードをあげます。
これを以下の要件に従った形式で訂正箇所を出力してください
{{
'severity': 'Info',
'message': 理由,
'line': 該当箇所の行数,
'end_line': 該当箇所の終わりの行数,
'column': 該当箇所の行内の文字数,
'end_column': 該当箇所の終わりの文字数,

}}[]
のフォーマットで出力

#要件
・json部分のみを出力すること。冒頭の日本語は絶対に出力しない。
・入力されたコードは、「Unity」での環境を前提としている。Unityで使用したときのことを考慮し、下記のルールに従って出力する。例えば、変数名やメソッドなど、Unityでよく書かれる書き方の場合は例外として下記の要件に従わなくてよい。
・messageについて
前提として、番号順に優先順位を設けている。
①必ずしも指摘すべき箇所を見つける必要はありません。もし可読性の面でクリティカル問題がない場合、json形式で「[]」と出力してください。
②あなたはプロの先生です。日本人の中学生や高校生がプログラミングの学習を行っていることを前提として、優しくアドバイスをするような口調で丁寧語を使わずに「～だよ！」や「～だね！」を利用して出力してください。決して生徒を傷つけてはいけません。
③文末は「！」にして、より表現を柔らかくしてください。
④変数名の意味が全く意味を持たず、読みやすさに著しく影響を与えている場合、「その変数名が誤っている理由」を出力して下さい。ただし、略称などについては明らかに意味が通っている場合(RigidBody -> rbなど)は意味がわかるので指摘しない。また、columnとend_columnについては、変数名のみを指定し、型などは含めないこと。
⑤一つのメソッドに対して同じ意味のコードがあったり、分けるべき処理が多く発見され、可読性に'著しく'障害をもたらしている場合、「不適切だと判定した理由」を出力してください。なお、StartやUpdate, fixedUpdateといったデフォルトのイベント関数についてはその限りではない。また、lineとend_lineについては、そのメソッドのアクセス修飾子から「{{」までを指定する。
⑥if文のネストなどの点で'著しく'読みにくい場合、「if文をきれいにまとめるコツ」を出力してください。また、lineとend_lineについては、if文の条件式部分のみを指定し、「}}」は含めないこと。

#コード
{code}
""";
            var content = new StringContent(
                JsonConvert.SerializeObject(new { prompt, ur_id }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var resStr = responseContent.Replace("\\n", "");
                var jsonResponse = JsonConvert.DeserializeObject<ApiResponse>(resStr)!;
                var feedbackStr = jsonResponse
                    .Response!.Replace("\"", "")
                    .Replace("'severity'", "\"severity\"")
                    .Replace("'message'", "\"message\"")
                    .Replace("'line'", "\"line\"")
                    .Replace("'end_line'", "\"end_line\"")
                    .Replace("'column'", "\"column\"")
                    .Replace("'end_column'", "\"end_column\"")
                    .Replace(": '", ": \"")
                    .Replace("',", "\",");

                if (feedbackStr == "[]")
                {
                    return [];
                }
                var feedback = JsonConvert.DeserializeObject<List<Issue>>(feedbackStr);
                return feedback!;
            }
            else
            {
                return null!;
            }
        }

        private static string AddLineNumbers(string code)
        {
            var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = $"{i + 1}.{lines[i]}";
            }
            return string.Join(Environment.NewLine, lines);
        }
    }

    public class ApiResponse
    {
        [JsonProperty("response")]
        public string? Response { get; set; }
    }
}
