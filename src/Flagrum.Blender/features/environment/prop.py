from dataclasses import dataclass


@dataclass(init=False)
class Prop:
    type: str
    name: str
    uri: str
    relative_path: str
    position: list[float]
    rotation: list[float]
    scale: list[float]
    prefab_rotations: list[list[float]]

    @staticmethod
    def from_dict(dictionary: dict):
        prop = Prop()
        prop.type = "EnvironmentProp"
        prop.name = dictionary["Name"]
        prop.uri = dictionary["Uri"]
        prop.relative_path = dictionary["RelativePath"]
        prop.position = dictionary["Position"]
        prop.rotation = dictionary["Rotation"]
        prop.scale = dictionary["Scale"]
        prop.prefab_rotations = dictionary["PrefabRotations"]
        return prop
