// LeetCode 1679 - Max Number of K-Sum Pairs
// Technique: Two Pointers (sort, then move one pointer from each end toward the middle).
// Time: O(n log n) for the sort, O(n) for the scan. Extra space: O(1).

public class Solution
{
    public int MaxOperations(int[] nums, int k)
    {
        Array.Sort(nums);

        int left = 0;
        int right = nums.Length - 1;
        int operations = 0;

        while (left < right)
        {
            // long: nums[i] can be up to 1e9, so the sum can reach 2e9 (close to int.MaxValue).
            long sum = (long)nums[left] + nums[right];

            if (sum == k)
            {
                // Found a pair: remove both numbers.
                operations++;
                left++;
                right--;
            }
            else if (sum < k)
            {
                // Even the largest remaining number is too small for nums[left],
                // so nums[left] can never be part of a pair. Skip it.
                left++;
            }
            else
            {
                // Even the smallest remaining number is too large for nums[right],
                // so nums[right] can never be part of a pair. Skip it.
                right--;
            }
        }

        return operations;
    }
}
