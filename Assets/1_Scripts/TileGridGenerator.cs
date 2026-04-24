using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileGridGenerator : MonoBehaviour
{
    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Material _innerMaterial;
    [SerializeField] private Material _outerMaterial;

    [Header("Settings")]
    [SerializeField] private GameObject _floorsParent;
    [SerializeField] private Vector3 _gridPosition = Vector3.zero;
    [SerializeField] private int _gridWidth = 5;
    [SerializeField] private int _gridHeight = 5;
    [SerializeField] private bool _outLineMode = false;
    [SerializeField] private bool _isEndFloor = false;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
#if UNITY_EDITOR
        if (_tilePrefab == null)
        {
            Debug.LogError("Tile Prefab이 할당되지 않았습니다.");
            return;
        }

        // 1. 모든 프리팹이 포함될 비어있는 하나의 부모 생성
        GameObject parentObj = new GameObject("TileGrid_Parent");
        Undo.RegisterCreatedObjectUndo(parentObj, "Create Tile Grid Parent");

        if (_floorsParent != null)
        {
            parentObj.transform.SetParent(_floorsParent.transform);
        }

        // 부모 오브젝트의 위치를 지정한 좌표로 초기화 및 레이어 지정
        parentObj.transform.localPosition = _gridPosition;
        int targetLayer = _isEndFloor ? 10 : 11;
        parentObj.layer = targetLayer;

        // 프리팹 Transform의 크기가 xyz 2칸을 나타냄a
        const float TileSize = 2f;

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                if (_outLineMode == true)
                {
                    bool isOutline = (x == 0 || x == _gridWidth - 1 || y == 0 || y == _gridHeight - 1);
                    if (isOutline == false)
                    {
                        continue;
                    }
                }

                // 타일의 중심을 기준으로 배치하되, 모서리가 (0,0)에 오도록 절반(TileSize * 0.5f)만큼 보정
                Vector3 localPos = new Vector3(
                    (x * TileSize) + (TileSize * 0.5f), 
                    (y * TileSize) + (TileSize * 0.5f), 
                    0f);
                
                // 단일 프리팹 인스턴스화 (부모도 함께 지정)
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(_tilePrefab, parentObj.transform);
                tile.transform.localPosition = localPos;
                tile.transform.localRotation = Quaternion.identity; // 원본 프리팹에 남아있을 수 있는 y축 등의 회전값 초기화
                tile.layer = 11; // 각 타일에도 Floor 레이어 설정

                // 마테리얼 인덱스 할당 로직
                MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    bool isLeft = (x == 0);
                    bool isRight = (x == _gridWidth - 1);
                    bool isBottom = (y == 0);
                    bool isTop = (y == _gridHeight - 1);

                    Material[] mats = meshRenderer.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        // 일단 모든 슬롯을 내부(Inner) 마테리얼로 초기화
                        mats[i] = _innerMaterial;
                    }
                    
                    // 해당 타일이 어느 방향 가장자리에 있는지에 따라, 블록의 모서리까지 포함하여 면 전체를 바깥 테두리로 칠함
                    // (1:좌상, 2:우상, 3:상, 4:하, 5:우하, 6:우, 7:좌하, 8:좌)
                    if (isTop)
                    {
                        if (1 < mats.Length) mats[1] = _outerMaterial;
                        if (3 < mats.Length) mats[3] = _outerMaterial;
                        if (2 < mats.Length) mats[2] = _outerMaterial;

                        if (_outLineMode == true)
                        {
                            if (7 < mats.Length) mats[7] = _outerMaterial;
                            if (4 < mats.Length) mats[4] = _outerMaterial;
                            if (5 < mats.Length) mats[5] = _outerMaterial;
                        }
                    }
                    if (isBottom)
                    {
                        if (7 < mats.Length) mats[7] = _outerMaterial;
                        if (4 < mats.Length) mats[4] = _outerMaterial;
                        if (5 < mats.Length) mats[5] = _outerMaterial;

                        if (_outLineMode == true)
                        {
                            if (1 < mats.Length) mats[1] = _outerMaterial;
                            if (3 < mats.Length) mats[3] = _outerMaterial;
                            if (2 < mats.Length) mats[2] = _outerMaterial;
                        }
                    }
                    if (isLeft)
                    {
                        if (1 < mats.Length) mats[1] = _outerMaterial;
                        if (8 < mats.Length) mats[8] = _outerMaterial;
                        if (7 < mats.Length) mats[7] = _outerMaterial;

                        if (_outLineMode == true)
                        {
                            if (2 < mats.Length) mats[2] = _outerMaterial;
                            if (6 < mats.Length) mats[6] = _outerMaterial;
                            if (5 < mats.Length) mats[5] = _outerMaterial;
                        }
                    }
                    if (isRight)
                    {
                        if (2 < mats.Length) mats[2] = _outerMaterial;
                        if (6 < mats.Length) mats[6] = _outerMaterial;
                        if (5 < mats.Length) mats[5] = _outerMaterial;

                        if (_outLineMode == true)
                        {
                            if (1 < mats.Length) mats[1] = _outerMaterial;
                            if (8 < mats.Length) mats[8] = _outerMaterial;
                            if (7 < mats.Length) mats[7] = _outerMaterial;
                        }
                    }

                    // 에디터 생성 로직이므로 인스턴싱 방지를 위해 sharedMaterials 활용
                    meshRenderer.sharedMaterials = mats;
                }
            }
        }

        // 2. 부모 오브젝트에 Rigidbody 추가
        Rigidbody rigidBody = Undo.AddComponent<Rigidbody>(parentObj);
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        // 3. 콜라이더 크기 및 중심 연산
        // 개수 * 각 크기(2)
        float totalSizeX = _gridWidth * TileSize;
        float totalSizeY = _gridHeight * TileSize;
        float totalSizeZ = TileSize; // z축(깊이)은 1칸이므로 2로 고정

        // 부모의 (0,0,0) 위치가 좌측 하단 모서리가 되었으므로 중심점은 전체 크기의 절반
        float centerX = totalSizeX * 0.5f;
        float centerY = totalSizeY * 0.5f;
        float centerZ = 0f; // 프리팹의 Z축 피봇이 중심이라고 가정

        if (_outLineMode == true)
        {
            // 4면 콜라이더 분리 (상하좌우 4개의 콜라이더로 구성)
            // Top (가로 전체, 세로 1칸)
            BoxCollider topCol = Undo.AddComponent<BoxCollider>(parentObj);
            topCol.size = new Vector3(totalSizeX, TileSize, totalSizeZ);
            topCol.center = new Vector3(centerX, totalSizeY - (TileSize * 0.5f), centerZ);

            // Bottom (가로 전체, 세로 1칸)
            BoxCollider bottomCol = Undo.AddComponent<BoxCollider>(parentObj);
            bottomCol.size = new Vector3(totalSizeX, TileSize, totalSizeZ);
            bottomCol.center = new Vector3(centerX, TileSize * 0.5f, centerZ);

            // Left & Right (상하에 1칸씩 차지했으므로, 남은 세로 길이만큼 가운데 배치)
            float verticalInnerSum = totalSizeY - (TileSize * 2f);
            if (verticalInnerSum > 0f)
            {
                // Left
                BoxCollider leftCol = Undo.AddComponent<BoxCollider>(parentObj);
                leftCol.size = new Vector3(TileSize, verticalInnerSum, totalSizeZ);
                leftCol.center = new Vector3(TileSize * 0.5f, centerY, centerZ);

                // Right
                BoxCollider rightCol = Undo.AddComponent<BoxCollider>(parentObj);
                rightCol.size = new Vector3(TileSize, verticalInnerSum, totalSizeZ);
                rightCol.center = new Vector3(totalSizeX - (TileSize * 0.5f), centerY, centerZ);
            }
        }
        else
        {
            // 단일 통합 BoxCollider 추가
            BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(parentObj);
            boxCollider.size = new Vector3(totalSizeX, totalSizeY, totalSizeZ);
            boxCollider.center = new Vector3(centerX, centerY, centerZ);
        }

        // 생성 직후 부모 오브젝트를 선택하여 바로 결과를 확인할 수 있도록 설정
        Selection.activeGameObject = parentObj;
        
        Debug.Log($"[{_gridWidth}x{_gridHeight}] 프리팹 기반의 타일 그리드가 완성되었습니다.");
#else
        Debug.LogWarning("에디터에서만 실행 가능한 기능입니다.");
#endif
    }
}
