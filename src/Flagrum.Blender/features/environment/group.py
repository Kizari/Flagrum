from dataclasses import dataclass

from .prop import Prop


@dataclass(init=False)
class Group:
    type: str
    name: str
    children: list[any]

    @staticmethod
    def from_dict(dictionary: dict):
        group = Group()
        group.type = "EnvironmentGroup"
        group.name = dictionary["Name"]
        group.children = []

        for child in dictionary["Children"]:
            if child["Type"] == "EnvironmentGroup":
                group.children.append(Group.from_dict(child))
            elif child["Type"] == "EnvironmentProp":
                group.children.append(Prop.from_dict(child))

        return group
