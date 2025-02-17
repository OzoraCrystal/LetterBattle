using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;
    
    // 状态切换事件
    public event System.Action<EnemyState> OnStateChanged;

    public void TransitionTo(EnemyState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(newState);
        OnStateChanged?.Invoke(newState);
    }

    private void EnterState(EnemyState state)
    {
        // 可添加状态进入时的特殊逻辑
    }

    private void ExitState(EnemyState state)
    {
        // 可添加状态退出时的清理逻辑
    }
}