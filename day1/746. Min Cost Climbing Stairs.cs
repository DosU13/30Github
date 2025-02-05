public class Solution {
    public int MinCostClimbingStairs(int[] cost) {
        int pre = 0, prepre = 0;
        foreach(var x in cost){
            var newCost = Math.Min(pre, prepre) + x;
            prepre = pre;
            pre = newCost;
        }
        return Math.Min(pre, prepre);
    }
}