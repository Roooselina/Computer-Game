using UnityEngine;

namespace EW
{
    /// <summary>
    /// 캐릭터 한 명의 그래픽 묶음. 게임 로직은 이 에셋과 상태 이름(Idle, Walk, ...)만 안다.
    /// 원화를 교체할 때는 controller(AnimatorOverrideController)의 클립만 바꾸면 된다.
    /// Docs/ART_REPLACEMENT_GUIDE.md 참고.
    /// </summary>
    [CreateAssetMenu(menuName = "EW/Character Art", fileName = "Art_")]
    public class CharacterArt : ScriptableObject
    {
        public string id;
        public string displayName;
        public RuntimeAnimatorController controller;
        public Sprite defaultSprite;
        [Tooltip("그림의 기본 방향이 오른쪽이면 true.")]
        public bool facesRight = true;
        [Tooltip("왼쪽을 볼 때 좌우 반전으로 처리. 비대칭 캐릭터는 leftController 를 따로 지정한다.")]
        public bool flipForLeft = true;
        [Tooltip("(선택) 왼쪽 전용 애니메이션. 지정하면 반전 대신 이것을 쓴다.")]
        public RuntimeAnimatorController leftController;
        [Tooltip("월드 기준 키(유닛). 말풍선·조사 표시 위치 계산용.")]
        public float height = 1.5f;
        [Tooltip("공중에 떠 있는 캐릭터의 기본 높이(유닛).")]
        public float hover;
        public int sortingOrder = 10;
    }
}
