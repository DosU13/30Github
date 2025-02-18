public class Solution {
    public int UniquePaths(int m, int n) {
        return Combination(n-1, m+n-2);
    }

    private int Combination(int k, int n){
        double res = 1;
        if (k > n-k) k = n-k;
        for(int i=0; i<k; i++){
            res = res * (n-i) / (i+1);
        }
        return (int)res;
    }
}