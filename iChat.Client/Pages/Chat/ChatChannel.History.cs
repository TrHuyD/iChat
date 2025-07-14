﻿namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        // History-related fields
        private bool _isLoading = false;
        private bool _isOldHistoryRequestButtonDisabled = false;
        private int _currentBucketIndex = 0;
        private async Task TriggerLoadOlderHistoryRequest()
        {
            if (_currentBucketIndex == 0)
            {
                _isOldHistoryRequestButtonDisabled = true;
                return;
            }
            if (_isLoading || _isOldHistoryRequestButtonDisabled)
                return;

            _isLoading = true;
            try
            {
                var result = await MessageManager.GetPreviousBucket(ChannelId, _currentBucketIndex);
                try
                {
                    await AddMessages(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                _currentBucketIndex = result.BucketId;
                Console.WriteLine($"Loaded previous bucket with ID: {_currentBucketIndex}");

                if (_currentBucketIndex == 0)
                {
                    DisableSpecialButtonPermanently();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Special request failed: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }
        private void DisableSpecialButtonPermanently()
        {
            _isOldHistoryRequestButtonDisabled = true;
        }
    }
}
