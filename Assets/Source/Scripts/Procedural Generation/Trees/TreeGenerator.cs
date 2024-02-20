using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public NoiseSettings treeNoiseSettings; // Настройки шума для деревьев
    public DomainWarping domainWrapping; // Искажение домена для деревьев

    // Генерация данных о деревьях
    public TreeData GenerateTreeData(ChunkData chunkData, Vector2Int mapSeedOffset)
    {
        treeNoiseSettings.worldOffset = mapSeedOffset; // Установка смещения мира для шума деревьев
        TreeData treeData = new TreeData(); // Создание объекта данных о деревьях
        float[,] noiseData = GenerateTreeNoise(chunkData, treeNoiseSettings); // Генерация шума для деревьев
        treeData.treePositions = DataPorocessing.FindLocalMaxima(noiseData, chunkData.worldPosition.x, chunkData.worldPosition.z); // Поиск локальных максимумов для определения позиций деревьев
        return treeData; // Возврат данных о деревьях
    }

    // Генерация шума для деревьев
    private float[,] GenerateTreeNoise(ChunkData chunkData, NoiseSettings treeNoiseSettings)
    {
        float[,] noiseMax = new float[chunkData.chunkSize, chunkData.chunkSize]; // Массив для хранения значения шума
        int xMax = chunkData.worldPosition.x + chunkData.chunkSize; // Максимальная координата X
        int xMin = chunkData.worldPosition.x; // Минимальная координата X
        int zMax = chunkData.worldPosition.z + chunkData.chunkSize; // Максимальная координата Z
        int zMin = chunkData.worldPosition.z; // Минимальная координата Z
        int xIndex = 0, zIndex = 0; // Индексы для заполнения массива шума
        for (int x = xMin; x < xMax; x++)
        {
            for (int z = zMin; z < zMax; z++)
            {
                noiseMax[xIndex, zIndex] = domainWrapping.GenerateDomainNoise(x, z, treeNoiseSettings); // Генерация шума с использованием искажения домена
                zIndex++; // Увеличение индекса Z
            }
            xIndex++; // Увеличение индекса X
            zIndex = 0; // Сброс индекса Z
        }
        return noiseMax; // Возврат массива шума
    }
}