public class Solution {
    private string w1, w2; 
    private int[,] dp;

    public int MinDistance(string word1, string word2) {
        w1 = word1;
        w2 = word2;
        dp = new int[w1.Length+1, w2.Length+2];
        for(int i=0; i<=w1.Length; i++){
            dp[i, w2.Length] = w1.Length - i;
        }
        for(int i=0; i<=w2.Length; i++){
            dp[w1.Length, i] = w2.Length - i;
        }
        for(int i = w1.Length-1; i>=0; i--){
            for(int j = w2.Length-1; j>=0; j--){
                if (w1[i] == w2[j]) dp[i, j] = dp[i+1, j+1];
                else dp[i, j] = Math.Min(Math.Min(dp[i, j+1], dp[i+1, j]), dp[i+1, j+1]) + 1;
            }
        }
        return dp[0, 0];
    }
}
