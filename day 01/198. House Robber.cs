public class Solution {
    public int Rob(int[] nums) {
        int maxWithLast = 0, maxWithoutLast = 0;
        foreach(var x in nums){
            var newMaxWithLast = maxWithoutLast + x;
            var newMaxWithoutLast = Math.Max(maxWithLast, maxWithoutLast);
            maxWithLast = newMaxWithLast;
            maxWithoutLast = newMaxWithoutLast;
        }
        return Math.Max(maxWithLast, maxWithoutLast);
    }
}