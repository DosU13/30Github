public class Solution {
    public int SumOfGoodNumbers(int[] nums, int k) {
        int res = 0;
        for(int i=0; i<nums.Length; i++){
            // var l = i-k<0 ? int.Max : nums[i-k];
            // var r = i+k>
            
            if ((i-k < 0 || nums[i-k] < nums[i])  &&
                (i+k >= nums.Length || nums[i+k] < nums[i])){
                res += nums[i];
            }
        }
        return res;
    }
}
