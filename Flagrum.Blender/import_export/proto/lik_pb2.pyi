from google.protobuf.internal import containers as _containers
from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Iterable as _Iterable, Mapping as _Mapping, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor
EYE_ID: IKPartType
HEAD_ID: IKPartType
JOINT_TYPE_BALL_AND_SOCKET: IKJointType
JOINT_TYPE_MINUS_Z_AXIS_ROTATION: IKJointType
JOINT_TYPE_X_AXIS_ROTATION: IKJointType
JOINT_TYPE_Y_AXIS_ROTATION: IKJointType
JOINT_TYPE_Z_AXIS_ROTATION: IKJointType
LEFT_ARM_ID: IKPartType
LEFT_FORELEG_ID: IKPartType
LEFT_HIND_LEG_ID: IKPartType
LEFT_LEG_ID: IKPartType
LEFT_WING_ID: IKPartType
NECK_ID: IKPartType
OTHER_LIMB_ID: IKPartType
RIGHT_ARM_ID: IKPartType
RIGHT_FORELEG_ID: IKPartType
RIGHT_HIND_LEG_ID: IKPartType
RIGHT_LEG_ID: IKPartType
RIGHT_WING_ID: IKPartType
ROOT_ID: IKPartType
SPINE_ID: IKPartType
TAIL_ID: IKPartType

class IKDepthLayer(_message.Message):
    __slots__ = ["joints"]
    JOINTS_FIELD_NUMBER: _ClassVar[int]
    joints: _containers.RepeatedScalarFieldContainer[int]
    def __init__(self, joints: _Optional[_Iterable[int]] = ...) -> None: ...

class IKFrame(_message.Message):
    __slots__ = ["position", "rotation"]
    POSITION_FIELD_NUMBER: _ClassVar[int]
    ROTATION_FIELD_NUMBER: _ClassVar[int]
    position: IKVector3
    rotation: IKQuaternion
    def __init__(self, position: _Optional[_Union[IKVector3, _Mapping]] = ..., rotation: _Optional[_Union[IKQuaternion, _Mapping]] = ...) -> None: ...

class IKJoint(_message.Message):
    __slots__ = ["bindPose", "children", "modelBoneId", "modelBoneName", "nonIKChildren", "nonIKChildrenIds", "parent", "type", "unknown"]
    BINDPOSE_FIELD_NUMBER: _ClassVar[int]
    CHILDREN_FIELD_NUMBER: _ClassVar[int]
    MODELBONEID_FIELD_NUMBER: _ClassVar[int]
    MODELBONENAME_FIELD_NUMBER: _ClassVar[int]
    NONIKCHILDRENIDS_FIELD_NUMBER: _ClassVar[int]
    NONIKCHILDREN_FIELD_NUMBER: _ClassVar[int]
    PARENT_FIELD_NUMBER: _ClassVar[int]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    UNKNOWN_FIELD_NUMBER: _ClassVar[int]
    bindPose: IKFrame
    children: _containers.RepeatedScalarFieldContainer[int]
    modelBoneId: int
    modelBoneName: str
    nonIKChildren: _containers.RepeatedScalarFieldContainer[str]
    nonIKChildrenIds: _containers.RepeatedScalarFieldContainer[int]
    parent: int
    type: IKJointType
    unknown: float
    def __init__(self, type: _Optional[_Union[IKJointType, str]] = ..., modelBoneId: _Optional[int] = ..., modelBoneName: _Optional[str] = ..., nonIKChildrenIds: _Optional[_Iterable[int]] = ..., parent: _Optional[int] = ..., children: _Optional[_Iterable[int]] = ..., nonIKChildren: _Optional[_Iterable[str]] = ..., unknown: _Optional[float] = ..., bindPose: _Optional[_Union[IKFrame, _Mapping]] = ...) -> None: ...

class IKJointFrame(_message.Message):
    __slots__ = ["frame", "joint"]
    FRAME_FIELD_NUMBER: _ClassVar[int]
    JOINT_FIELD_NUMBER: _ClassVar[int]
    frame: IKFrame
    joint: int
    def __init__(self, frame: _Optional[_Union[IKFrame, _Mapping]] = ..., joint: _Optional[int] = ...) -> None: ...

class IKLookingElement(_message.Message):
    __slots__ = ["eyes", "head"]
    EYES_FIELD_NUMBER: _ClassVar[int]
    HEAD_FIELD_NUMBER: _ClassVar[int]
    eyes: _containers.RepeatedCompositeFieldContainer[IKJointFrame]
    head: IKJointFrame
    def __init__(self, head: _Optional[_Union[IKJointFrame, _Mapping]] = ..., eyes: _Optional[_Iterable[_Union[IKJointFrame, _Mapping]]] = ...) -> None: ...

class IKPart(_message.Message):
    __slots__ = ["type"]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    type: IKPartType
    def __init__(self, type: _Optional[_Union[IKPartType, str]] = ...) -> None: ...

class IKQuaternion(_message.Message):
    __slots__ = ["w", "x", "y", "z"]
    W_FIELD_NUMBER: _ClassVar[int]
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    Z_FIELD_NUMBER: _ClassVar[int]
    w: float
    x: float
    y: float
    z: float
    def __init__(self, x: _Optional[float] = ..., y: _Optional[float] = ..., z: _Optional[float] = ..., w: _Optional[float] = ...) -> None: ...

class IKRig(_message.Message):
    __slots__ = ["jointLayers", "joints", "lookingElements", "parts", "rootJoint"]
    JOINTLAYERS_FIELD_NUMBER: _ClassVar[int]
    JOINTS_FIELD_NUMBER: _ClassVar[int]
    LOOKINGELEMENTS_FIELD_NUMBER: _ClassVar[int]
    PARTS_FIELD_NUMBER: _ClassVar[int]
    ROOTJOINT_FIELD_NUMBER: _ClassVar[int]
    jointLayers: _containers.RepeatedCompositeFieldContainer[IKDepthLayer]
    joints: _containers.RepeatedCompositeFieldContainer[IKJoint]
    lookingElements: _containers.RepeatedCompositeFieldContainer[IKLookingElement]
    parts: _containers.RepeatedCompositeFieldContainer[IKPart]
    rootJoint: int
    def __init__(self, joints: _Optional[_Iterable[_Union[IKJoint, _Mapping]]] = ..., parts: _Optional[_Iterable[_Union[IKPart, _Mapping]]] = ..., jointLayers: _Optional[_Iterable[_Union[IKDepthLayer, _Mapping]]] = ..., rootJoint: _Optional[int] = ..., lookingElements: _Optional[_Iterable[_Union[IKLookingElement, _Mapping]]] = ...) -> None: ...

class IKVector3(_message.Message):
    __slots__ = ["x", "y", "z"]
    X_FIELD_NUMBER: _ClassVar[int]
    Y_FIELD_NUMBER: _ClassVar[int]
    Z_FIELD_NUMBER: _ClassVar[int]
    x: float
    y: float
    z: float
    def __init__(self, x: _Optional[float] = ..., y: _Optional[float] = ..., z: _Optional[float] = ...) -> None: ...

class IKJointType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = []

class IKPartType(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = []
