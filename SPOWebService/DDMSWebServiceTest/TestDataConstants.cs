namespace ResponseServiceUnitTests
{
    public static class TestDataConstants
    {
        public static string TargetResponse = "{{\"batch_header\":{{\"payout_batch_id\":\"testbatchid\",\"batch_status\":\"PENDING\",\"sender_batch_header\":{{\"sender_batch_id\":\"FT_48a6388d2e80464096bf0473322b9485\",\"email_subject\":\"You have a payment\",\"recipient_type\":\"EMAIL\"}}}},\"links\":[{{\"href\":\"{0}{1}\",\"rel\":\"self\",\"method\":\"GET\",\"enctype\":\"application/json\"}}]}}";
    }
}
