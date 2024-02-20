using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public NoiseSettings treeNoiseSettings; // ��������� ���� ��� ��������
    public DomainWarping domainWrapping; // ��������� ������ ��� ��������

    // ��������� ������ � ��������
    public TreeData GenerateTreeData(ChunkData chunkData, Vector2Int mapSeedOffset)
    {
        treeNoiseSettings.worldOffset = mapSeedOffset; // ��������� �������� ���� ��� ���� ��������
        TreeData treeData = new TreeData(); // �������� ������� ������ � ��������
        float[,] noiseData = GenerateTreeNoise(chunkData, treeNoiseSettings); // ��������� ���� ��� ��������
        treeData.treePositions = DataPorocessing.FindLocalMaxima(noiseData, chunkData.worldPosition.x, chunkData.worldPosition.z); // ����� ��������� ���������� ��� ����������� ������� ��������
        return treeData; // ������� ������ � ��������
    }

    // ��������� ���� ��� ��������
    private float[,] GenerateTreeNoise(ChunkData chunkData, NoiseSettings treeNoiseSettings)
    {
        float[,] noiseMax = new float[chunkData.chunkSize, chunkData.chunkSize]; // ������ ��� �������� �������� ����
        int xMax = chunkData.worldPosition.x + chunkData.chunkSize; // ������������ ���������� X
        int xMin = chunkData.worldPosition.x; // ����������� ���������� X
        int zMax = chunkData.worldPosition.z + chunkData.chunkSize; // ������������ ���������� Z
        int zMin = chunkData.worldPosition.z; // ����������� ���������� Z
        int xIndex = 0, zIndex = 0; // ������� ��� ���������� ������� ����
        for (int x = xMin; x < xMax; x++)
        {
            for (int z = zMin; z < zMax; z++)
            {
                noiseMax[xIndex, zIndex] = domainWrapping.GenerateDomainNoise(x, z, treeNoiseSettings); // ��������� ���� � �������������� ��������� ������
                zIndex++; // ���������� ������� Z
            }
            xIndex++; // ���������� ������� X
            zIndex = 0; // ����� ������� Z
        }
        return noiseMax; // ������� ������� ����
    }
}