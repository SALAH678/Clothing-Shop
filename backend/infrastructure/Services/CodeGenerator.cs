using Application.Common.Interfaces;
using System.Security.Cryptography;

namespace infrastructure.Services;

public sealed class CodeGenerator : ICodeGenerator
{
    public string GenerateCode(int length = 6)
    {
        if (length <= 0)
            throw new ArgumentException(
                "Length must be greater than zero.");

        Span<char> code = stackalloc char[length];

        for (int i = 0; i < length; i++)
            code[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));

        return new string(code);
    }
}
