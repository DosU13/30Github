public class Solution {
    public int MaxProfit(int[] prices, int fee) {
        int maxProfit = 0, maxProfitWithStock = -50000;
        foreach(var x in prices){
            var newMaxProfit = maxProfitWithStock + x - fee;
            if (newMaxProfit > maxProfit) maxProfit = newMaxProfit;
            var newMaxProfitWithStock = maxProfit - x;
            if (newMaxProfitWithStock > maxProfitWithStock) maxProfitWithStock = newMaxProfitWithStock;
        }
        return maxProfit;
    }
}