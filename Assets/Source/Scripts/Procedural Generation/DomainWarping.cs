using UnityEngine;

public class DomainWarping : MonoBehaviour
{
    public NoiseSettings noiseDomainX, noiseDomainY; // Настройки шума для домена по X и Y
    public int amplitudeX = 20, amplitudeY = 20; // Амплитуды домена по X и Y

    // Генерация шума домена
    public float GenerateDomainNoise(int x, int z, NoiseSettings defaultNoiseSettings)
    {
        Vector2 domainOffset = GenerateDomainOffset(x, z);
        return MyNoise.OctavePerlin(x + domainOffset.x, z + domainOffset.y, defaultNoiseSettings);
    }

    // Генерация смещения домена
    public Vector2 GenerateDomainOffset(int x, int z)
    {
        var noiseX = MyNoise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
        var noiseY = MyNoise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
        return new Vector2(noiseX, noiseY);
    }

    // Генерация целочисленного смещения домена
    public Vector2Int GenerateDomainOffsetInt(int x, int z)
    {
        return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
    }
}