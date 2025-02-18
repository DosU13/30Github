public class Solution {
    public int[] CountBits(int n) {
        var res = new int[n+1];
        res[0] = 0;
        for(int i=1; i<=n; i*=2){
            res[i] = 1;
        }
        for(int i=1; i<=n; i++){
            if (res[i] != 1)
                res[i] = 2*res[i&(i-1)] + res[i^(i-1)] - res[i-1];
        }
        return res;
    }
}
