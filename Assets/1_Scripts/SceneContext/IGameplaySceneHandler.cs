using System;
using UnityEngine;

/// <summary>
/// 게임플레이 씬(Level, Tutorial 등)의 공통 인터페이스.
/// 맵 전환, 사망, 스테이지 클리어 등의 기능을 정의합니다.
/// MapTransitionTrigger, FinishTrigger, PlayerManager 등이 이 인터페이스를 통해
/// 구체적인 SceneContext 타입을 알 필요 없이 게임플레이 기능에 접근합니다.
/// </summary>
public interface IGameplaySceneHandler
{
    MapData CurrentMap { get; }
    Action<PlayerManager> OnPlayerDeath { get; set; }
    Action<LevelData, float> OnStageClear { get; set; }

    void TransitionToMap(MapData targetMap, GameObject player);
    void StageClear();
}
