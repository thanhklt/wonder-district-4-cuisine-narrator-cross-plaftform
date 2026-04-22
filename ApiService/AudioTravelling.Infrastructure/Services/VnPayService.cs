using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AudioTravelling.Infrastructure.Services;

public class VnPayService(IConfiguration config)
{
    private const string VERSION = "2.1.0";
    private const string COMMAND = "pay";
    private const string CURRENCY = "VND";
    private const string LOCALE = "vn";
    private const string ORDER_TYPE = "other";

    public string CreatePaymentUrl(string accessCode, decimal amount, string ipAddress, string txnRef)
    {
        var tmnCode = config["VNPAY_TMN_CODE"]!;
        var hashSecret = config["VNPAY_HASH_SECRET"]!;
        var vnpUrl = config["VNPAY_URL"]!;
        var returnUrl = config["VNPAY_RETURN_URL"]!;

        var createDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
        var expireDate = DateTime.UtcNow.AddHours(7).AddMinutes(15).ToString("yyyyMMddHHmmss");

        var data = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = VERSION,
            ["vnp_Command"] = COMMAND,
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_Amount"] = ((long)(amount * 100)).ToString(),
            ["vnp_CreateDate"] = createDate,
            ["vnp_CurrCode"] = CURRENCY,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_Locale"] = LOCALE,
            ["vnp_OrderInfo"] = $"Thanh toan AudioTravelling {accessCode}",
            ["vnp_OrderType"] = ORDER_TYPE,
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_ExpireDate"] = expireDate,
            ["vnp_TxnRef"] = txnRef,
        };

        var query = string.Join("&", data.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var hashData = string.Join("&", data.Select(kv => $"{kv.Key}={kv.Value}"));
        var secureHash = HmacSha512(hashSecret, hashData);

        return $"{vnpUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateCallback(IReadOnlyDictionary<string, string> queryParams)
    {
        var hashSecret = config["VNPAY_HASH_SECRET"]!;
        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash)) return false;

        var data = new SortedDictionary<string, string>(
            queryParams
                .Where(kv => kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash")
                .ToDictionary(kv => kv.Key, kv => kv.Value));

        var hashData = string.Join("&", data.Select(kv => $"{kv.Key}={kv.Value}"));
        var expectedHash = HmacSha512(hashSecret, hashData);
        return expectedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA512.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
