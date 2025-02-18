public class Solution {
    public int NumTilings(int n) {
        int mod = 1_000_000_007;
        long f = 2, pf = 1, g = 1;
        if(n<=2) return n;
        while(n-->2){
            (f, pf, g) = ((f + pf + 2*g)%mod, f, (pf + g)%mod);
        }
        return (int)f;
    }
}