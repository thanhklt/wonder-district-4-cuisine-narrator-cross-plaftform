namespace AudioTravelling.API.DTOs;

public record PayRequest(string Code);
public record PaymentResponse(string PaymentUrl, string TxnRef);
public record VerifyResponse(bool Valid, DateTime ExpiresAt);
public record BootstrapResponse(IEnumerable<PoiSummaryResponse> Pois, DateTime SessionExpiresAt);
