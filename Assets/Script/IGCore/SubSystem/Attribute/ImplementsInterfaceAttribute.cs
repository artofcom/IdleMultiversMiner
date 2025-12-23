using System;
using UnityEngine;

// 이 어트리뷰트는 MonoBehaviour 필드에만 적용되도록 합니다.
// allowMultiple=false는 하나의 필드에 여러 번 적용될 수 없음을 의미합니다.
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ImplementsInterfaceAttribute : PropertyAttribute
{
    public Type TargetType { get; private set; }

    public ImplementsInterfaceAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}