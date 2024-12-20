using Microsoft.AspNetCore.Mvc;
using Common.Core.Security.Interfaces;

namespace Common.TestApi.Controllers;

public class EncryptionController : BaseApiController
{
    private readonly IDataEncryptionService _dataEncryptionService;
    private readonly IPasswordHashingService _passwordHashingService;

    public EncryptionController(IDataEncryptionService dataEncryptionService, IPasswordHashingService passwordHashingService)
    {
        _dataEncryptionService = dataEncryptionService;
        _passwordHashingService = passwordHashingService;
    }

    [HttpPost("encrypt")]
    public IActionResult EncryptData([FromBody] EncryptRequest request)
    {
        var encryptedBytes = _dataEncryptionService.EncryptToBytes(request.PlainText);
        return Ok(Convert.ToBase64String(encryptedBytes));
    }

    [HttpPost("decrypt")]
    public IActionResult DecryptData([FromBody] DecryptRequest request)
    {
        var encryptedBytes = Convert.FromBase64String(request.EncryptedData);
        var decryptedText = _dataEncryptionService.DecryptFromBytes(encryptedBytes);
        return Ok(decryptedText);
    }

    [HttpPost("hash")]
    public IActionResult HashPassword([FromBody] HashRequest request)
    {
        var hashedPassword = _passwordHashingService.HashPassword(request.Password);
        return Ok(hashedPassword);
    }

    [HttpPost("verify")]
    public IActionResult VerifyPassword([FromBody] VerifyRequest request)
    {
        var isValid = _passwordHashingService.VerifyPassword(request.Password, request.HashedPassword);
        return Ok(isValid);
    }
}

public class EncryptRequest
{
    public string PlainText { get; set; }
}

public class DecryptRequest
{
    public string EncryptedData { get; set; }
}

public class HashRequest
{
    public string Password { get; set; }
}

public class VerifyRequest
{
    public string Password { get; set; }
    public string HashedPassword { get; set; }
}
