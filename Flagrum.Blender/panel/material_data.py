from bpy.props import (
    EnumProperty,
    FloatProperty,
    FloatVectorProperty,
    StringProperty,
    BoolProperty,
    CollectionProperty,
    IntProperty
)
from bpy.types import PropertyGroup

from ..entities import MaterialPropertyMetadata

material_enum = (
    ('NONE', 'None', 'No material'),
    ('NAMED_HUMAN_OUTFIT', 'Named Human Outfit', ''),
    ('NAMED_HUMAN_SKIN', 'Named Human Skin', ''),
    ('NAMED_HUMAN_HAIR', 'Named Human Hair', ''),
    ('NAMED_HUMAN_EYE', 'Named Human Eye', ''),
    ('NAMED_HUMAN_GLASS', 'Named Human Glass', ''),
    ('AVATAR_OUTFIT', 'Avatar Outfit', ''),
    ('AVATAR_SKIN', 'Avatar Skin', ''),
    ('AVATAR_CLOTH', 'Avatar Cloth', ''),
    ('AVATAR_HAIR', 'Avatar Hair', ''),
    ('AVATAR_EYE', 'Avatar Eye', ''),
    ('AVATAR_GLASS', 'Avatar Glass', ''),
    ('BASIC_MATERIAL', 'Basic Material', ''),
    ('GLASS_MATERIAL', 'Glass Material', '')
)

material_properties = {
    'NONE': [],
    'NAMED_HUMAN_OUTFIT': [MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                           MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                           MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                           MaterialPropertyMetadata('occlusion_set', False, 5, 'INPUT', 0),
                           MaterialPropertyMetadata('NormalMask_power', True, 29, 'INPUT', 0.8),
                           MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                           MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                           MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 0.3),
                           MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                           MaterialPropertyMetadata('SweatTextureUVScale', True, 110, 'INPUT', [1, 1]),
                           MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                           MaterialPropertyMetadata('FazzMask0_Power', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                           MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                           MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1.5, 1.5, 1.5]),
                           MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0.5),
                           MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                           MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [6, 6]),
                           MaterialPropertyMetadata('AlphaAllFadeInStartDistance', False, 999, 'INPUT', 0.1),
                           MaterialPropertyMetadata('SweatColor', False, 999, 'INPUT', [0.5, 0.5, 0.5]),
                           MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0.3),
                           MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0),
                           MaterialPropertyMetadata('AlphaAllFadeInEndDistance', False, 999, 'INPUT', 0.13),
                           MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 4]),
                           MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1.5, 1.5, 1.5]),
                           MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                           MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('FP_FadePower', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                           MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                           MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.024, 0.0281347, 0.04]),
                           MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 0.7),
                           MaterialPropertyMetadata('Fazz0_length', False, 999, 'INPUT', 0.5),
                           MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [0.7, 0.7, 0.7]),
                           MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.35),
                           MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [5, 5]),
                           MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [7, 8]),
                           MaterialPropertyMetadata('BloodAmount_R', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('DirtAmount_B', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('BloodAmount_B', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                           MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('BloodAmount_G', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                           MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                           MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                           MaterialPropertyMetadata('LayeredMask0_NormalTextures', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                           MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                           MaterialPropertyMetadata('SweatTexture_Mask', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('BloodTexture_Mask', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                           MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', ''),
                           MaterialPropertyMetadata('BloodTexture', False, 999, 'TEXTURE', ''),
                           MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'NAMED_HUMAN_SKIN': [MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('SkinBloodAmount_B', True, 10, 'INPUT', 0),
                         MaterialPropertyMetadata('AlphaAllFadeInStartDistance', False, 999, 'INPUT', 0.1),
                         MaterialPropertyMetadata('SkinBloodAmount_A', True, 11, 'INPUT', 0),
                         MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [4, 4]),
                         MaterialPropertyMetadata('SkinBloodAmount_R', True, 8, 'INPUT', 0),
                         MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [1.5, 1.5, 1.5]),
                         MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                         MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.0289, 0.0503639, 0.1]),
                         MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [4.2, 6]),
                         MaterialPropertyMetadata('AlphaAllFadeInEndDistance', False, 999, 'INPUT', 0.13),
                         MaterialPropertyMetadata('VertexColor_MaskControl', True, 68, 'INPUT', 0.8),
                         MaterialPropertyMetadata('map1_UVScale0', False, 999, 'INPUT', [4, 8]),
                         MaterialPropertyMetadata('SSSStrength', True, 6, 'INPUT', 1),
                         MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                         MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                         MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [2, 2]),
                         MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                         MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                         MaterialPropertyMetadata('SSAO_Blend', True, 67, 'INPUT', 0.5),
                         MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1.3),
                         MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                         MaterialPropertyMetadata('aoSaturation_Power', False, 999, 'INPUT', 2),
                         MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0),
                         MaterialPropertyMetadata('occlusion_set', True, 5, 'INPUT', 0),
                         MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 0.9),
                         MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.25),
                         MaterialPropertyMetadata('WetnessAbsorbency', True, 79, 'INPUT', 0),
                         MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                         MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [2, 2, 2]),
                         MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [2, 4]),
                         MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('FP_FadePower', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('SSS_HueColor', True, 7, 'INPUT', [1, 0, 0]),
                         MaterialPropertyMetadata('Normal1_Power', False, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.3),
                         MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                         MaterialPropertyMetadata('BaseColor1', False, 999, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                         MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1.5, 1.5, 1.5]),
                         MaterialPropertyMetadata('SkinDirtAmount_B', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0.25),
                         MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0.4),
                         MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                         MaterialPropertyMetadata('SkinBloodAmount_G', True, 9, 'INPUT', 0),
                         MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                         MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                         MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                         MaterialPropertyMetadata('SSSMask_Texture', True, 10, 'TEXTURE', ''),
                         MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('BloodTexture_Mask', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('Sweat_Textuer', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                         MaterialPropertyMetadata('MultiMask0_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('BaseColor1_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('OpacityMask0_Texture', False, 11, 'TEXTURE', ''),
                         MaterialPropertyMetadata('BloodTexture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'NAMED_HUMAN_HAIR': [MaterialPropertyMetadata('WetnessAbsorbency', True, 79, 'INPUT', 0.2),
                         MaterialPropertyMetadata('Emissive0_Power', False, 35, 'INPUT', 0),
                         MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 0.4),
                         MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                         MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                         MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                         MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0.55),
                         MaterialPropertyMetadata('BlendOut', True, 999, 'INPUT', 0.3),
                         MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('hairNoise_strength', True, 60, 'INPUT', 0.3),
                         MaterialPropertyMetadata('EffectBiass_length', True, 999, 'INPUT', 1.3),
                         MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0.4),
                         MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                         MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT',
                                                  [0.490043, 0.490043, 0.490043]),
                         MaterialPropertyMetadata('AlphaThresholdPrepass', True, 21, 'INPUT', 0.73),
                         MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                         MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 0.7),
                         MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                         MaterialPropertyMetadata('Emissive0_Color', False, 34, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('AlphaAllFadeInStartDistance', True, 999, 'INPUT', 0.1),
                         MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [2, 1]),
                         MaterialPropertyMetadata('AlphaThreshold_Shadow', True, 20, 'INPUT', 0.1),
                         MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('FP_FadePower', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.5),
                         MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                         MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.2),
                         MaterialPropertyMetadata('NoDither', True, 26, 'INPUT', 0),
                         MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [10, 10]),
                         MaterialPropertyMetadata('DirtAmount_B', False, 999, 'INPUT', 0),
                         MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [1, 1]),
                         MaterialPropertyMetadata('AlphaAllFadeInEndDistance', True, 999, 'INPUT', 0.13),
                         MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.4, 0.376849, 0.2788]),
                         MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                         MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 0.35),
                         MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 2]),
                         MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                         MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                         MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('HairMarschnerNTexture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                         MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', ''),
                         MaterialPropertyMetadata('hairNoiseTexture', False, 999, 'TEXTURE', ''),
                         MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'NAMED_HUMAN_EYE': [MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [1, 1]),
                        MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                        MaterialPropertyMetadata('AlphaAllFadeInEndDistance', False, 999, 'INPUT', 0.13),
                        MaterialPropertyMetadata('FP_FadePower', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('ShadowBlur', True, 65, 'INPUT', 0.05),
                        MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                        MaterialPropertyMetadata('refractionDepth', True, 64, 'INPUT', 1),
                        MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [1, 1, 1]),
                        MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                        MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                        MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                        MaterialPropertyMetadata('Hyperemia_Power', True, 62, 'INPUT', 0),
                        MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('EmissiveScale_Power', True, 999, 'INPUT', 1),
                        MaterialPropertyMetadata('SSAO_Blend', True, 67, 'INPUT', 0.5),
                        MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                        MaterialPropertyMetadata('MagicDamages_UVScale', False, 101, 'INPUT', [0, 0]),
                        MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                        MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                        MaterialPropertyMetadata('BaseColor1', True, 999, 'INPUT', [1, 1, 1]),
                        MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1.5, 1.5, 1.5]),
                        MaterialPropertyMetadata('Hyperemia_Color', True, 61, 'INPUT', [1, 1, 1]),
                        MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                        MaterialPropertyMetadata('AlphaThreshold', False, 19, 'INPUT', 0),
                        MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0),
                        MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                        MaterialPropertyMetadata('AlphaAllFadeInStartDistance', False, 999, 'INPUT', 0.1),
                        MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                        MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                        MaterialPropertyMetadata('Normal0InnerTexture', True, 21, 'TEXTURE', ''),
                        MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                        MaterialPropertyMetadata('RefractionMask0', True, 19, 'TEXTURE', ''),
                        MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                        MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                        MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                        MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                        MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                        MaterialPropertyMetadata('BaseColor0Texture', True, 2, 'TEXTURE', '')],
    'NAMED_HUMAN_GLASS': [MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                          MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                          MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                          MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                          MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                          MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                          MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                          MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                          MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                          MaterialPropertyMetadata('BaseColor1', False, 999, 'INPUT', [1, 1, 1]),
                          MaterialPropertyMetadata('Transparency_exp', True, 25, 'INPUT', 0),
                          MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                          MaterialPropertyMetadata('Occlusion1_Power', False, 999, 'INPUT', 1),
                          MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                          MaterialPropertyMetadata('Transparency_biass', True, 24, 'INPUT', 0.5),
                          MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                          MaterialPropertyMetadata('Transparency0_Power', True, 23, 'INPUT', 1),
                          MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 0.7),
                          MaterialPropertyMetadata('Normal1_Power', False, 999, 'INPUT', 1),
                          MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                          MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                          MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 1),
                          MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                          MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                          MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Normal1_Texture', False, 999, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Occlusion1_Texture', False, 999, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                          MaterialPropertyMetadata('MultiMask0_Texture', False, 999, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Transparency0_Texture', True, 12, 'TEXTURE', ''),
                          MaterialPropertyMetadata('Emissive0_Texture', True, 15, 'TEXTURE', ''),
                          MaterialPropertyMetadata('BaseColor1_Texture', False, 999, 'TEXTURE', '')],
    'AVATAR_OUTFIT': [MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                      MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                      MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0.1),
                      MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                      MaterialPropertyMetadata('NormalMask_power', True, 29, 'INPUT', 0.5),
                      MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [3.5, 3.5]),
                      MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [5, 5]),
                      MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                      MaterialPropertyMetadata('FP_FadePower', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                      MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                      MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                      MaterialPropertyMetadata('AlphaAllFadeInEndDistance', False, 999, 'INPUT', 0.13),
                      MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                      MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('Fazz0_length', False, 999, 'INPUT', 0.5),
                      MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                      MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                      MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                      MaterialPropertyMetadata('SweatColor', False, 999, 'INPUT', [0, 0, 0]),
                      MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.0588235, 0.0705882, 0.101961]),
                      MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                      MaterialPropertyMetadata('BloodAmount_B', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                      MaterialPropertyMetadata('BloodAmount_G', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.35),
                      MaterialPropertyMetadata('BloodAmount_R', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                      MaterialPropertyMetadata('DirtAmount_B', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                      MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                      MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                      MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 1),
                      MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                      MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('SweatTextureUVScale', True, 110, 'INPUT', [1, 1]),
                      MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [5.5, 5.5]),
                      MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 4]),
                      MaterialPropertyMetadata('FazzMask0_Power', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0),
                      MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                      MaterialPropertyMetadata('AlphaAllFadeInStartDistance', False, 999, 'INPUT', 0.1),
                      MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                      MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                      MaterialPropertyMetadata('LayeredMask0_NormalTextures', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                      MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('Occlusion1_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('DamageMask0_MMMM_Texture', True, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                      MaterialPropertyMetadata('SweatTexture_Mask', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('BloodTexture_Mask', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                      MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('Emissive0_Texture', True, 15, 'TEXTURE', ''),
                      MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', ''),
                      MaterialPropertyMetadata('BloodTexture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('DamageMask0_MMM_Texture', False, 999, 'TEXTURE', ''),
                      MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'AVATAR_SKIN': [MaterialPropertyMetadata('BaseColor_Tattoo_Front', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Roughness_Power_Makeup', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Metallic_Power_Makeup', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.03161, 0.054827, 0.109]),
                    MaterialPropertyMetadata('MultiMask_Power_Manicure', True, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Tex_index_Scar', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Roughness_Power_Hair', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('AlphaThreshold', False, 19, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor_Tattoo_Back', True, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Tex_index_Tattoo_Back', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('VertexColor_MaskControl', True, 68, 'INPUT', 0.7),
                    MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                    MaterialPropertyMetadata('BaseColor_Tattoo_Left', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [4, 4]),
                    MaterialPropertyMetadata('BakePreview', False, 999, 'INPUT', 1E-45),
                    MaterialPropertyMetadata('Emissive0_Power', False, 35, 'INPUT', 0),
                    MaterialPropertyMetadata('ShadowBlur', True, 65, 'INPUT', 1),
                    MaterialPropertyMetadata('MultiMask_UVScale_Stubble', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('UVScale_Makeup0', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.4),
                    MaterialPropertyMetadata('MultiMask_Power_Lip', True, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SkinBloodAmount_B', False, 10, 'INPUT', 0),
                    MaterialPropertyMetadata('Tex_index_Tattoo_Front', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SkinBloodAmount_A', False, 11, 'INPUT', 0),
                    MaterialPropertyMetadata('UVScale_MultiMask_Manicure', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('SkinBloodAmount_G', False, 9, 'INPUT', 0),
                    MaterialPropertyMetadata('map_UVScale_Color_Scar', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Tex_index_Occlusion_Shoes', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('ShadowBoundary_Saturation', True, 66, 'INPUT', 1),
                    MaterialPropertyMetadata('Tex_index_Eyebrow', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Occlusion_Power_Pants', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Normal1_Power', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Occlusion_Power_Glove', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('SkinDirtAmount_B', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor_Stubble', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('map1_UVScale_Makeup', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [5.5, 5.7]),
                    MaterialPropertyMetadata('SkinBloodAmount_R', False, 8, 'INPUT', 0),
                    MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('WetnessAbsorbency', False, 79, 'INPUT', 0),
                    MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0.4),
                    MaterialPropertyMetadata('map_UVScale_Normal_Scar', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Tex_index_Occlusion_Pants', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('DefaultLipColor', True, 999, 'INPUT', [0.69, 0.4209, 0.4209]),
                    MaterialPropertyMetadata('MultiMask_R_Power_wrinkle', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('UVScale_MultiMask_Eyebrow', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                    MaterialPropertyMetadata('SSSStrength', True, 6, 'INPUT', 1),
                    MaterialPropertyMetadata('MultiMask0_Power', True, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Specular_Power_Hair', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('BaseColor_Scar', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('UVScale_Tattoo_Front', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Specular_Power_Manicure', True, 999, 'INPUT', 0.5),
                    MaterialPropertyMetadata('UVScale_Color_WhiteHair', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power_Tattoo_Back', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('map_UVScale_Eyebrow_Shadow', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('map_UVScale_Tattoo_Left', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Emissive0_Color', False, 34, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                    MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 4]),
                    MaterialPropertyMetadata('Curvature_Bias0', True, 999, 'INPUT', 0.5),
                    MaterialPropertyMetadata('Specular_Power_Makeup', True, 999, 'INPUT', 0.5),
                    MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Tex_index_Tattoo_Right', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('DefaultSkinColor', True, 999, 'INPUT', [0.69, 0.4209, 0.4209]),
                    MaterialPropertyMetadata('MultiMask_Power_Eyebrow', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Normal_UVScale_Stubble', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Roughness_Power_Manicure', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Tex_index_Stubble', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power_Tattoo_Left', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Normal_UVScale_Hair', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0.4),
                    MaterialPropertyMetadata('BaseColor_Eyebrow', True, 999, 'INPUT', [0.094, 0.083772, 0.061382]),
                    MaterialPropertyMetadata('MultiMask_Power_Tattoo_Front', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Occlusion_Power_Shoes', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('MultiMask_Power_WhiteEyebrow', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('map_UVScale_Tattoo_Right', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Tex_index_Makeup', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power_Tattoo_Right', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power_Hair', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('UVScale_Makeup', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                    MaterialPropertyMetadata('BaseColor_Hair', True, 999, 'INPUT',
                                             [0.00585946, 0.00585946, 0.00585946]),
                    MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.1),
                    MaterialPropertyMetadata('Occlusion_Power_Wear', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('LipColor', False, 999, 'INPUT', [0.69, 0.4209, 0.4209]),
                    MaterialPropertyMetadata('MultiMask_Power_Stubble', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor_WhiteEyebrow', False, 999, 'INPUT', [0, 0, 0]),
                    MaterialPropertyMetadata('SSS_HueColor', True, 7, 'INPUT', [1, 0, 0]),
                    MaterialPropertyMetadata('Tex_index_Occlusion_Tops', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SSAO_Blend', True, 67, 'INPUT', 0.5),
                    MaterialPropertyMetadata('Occlusion_Power_Hair', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('MultiMask_Power_Scar', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor_Makeup', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Tex_index_Occlusion_Wear', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SkinColor', False, 999, 'INPUT', [0.69, 0.4209, 0.4209]),
                    MaterialPropertyMetadata('Metallic_Power_Hair', True, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor0', False, 1, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                    MaterialPropertyMetadata('Metallic_Power_Manicure', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_UVScale_Hair', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [1, 0.846667, 0.8]),
                    MaterialPropertyMetadata('BaseColor_Manicure', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                    MaterialPropertyMetadata('MultiMask_Power_WhiteHair', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SSS_normalMipBias', False, 999, 'INPUT', 5),
                    MaterialPropertyMetadata('UVScale_Color_WhiteEyebrow', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('MultiMask_Power_Makeup', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_G_Power_wrinkle', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                    MaterialPropertyMetadata('map1_UVScale2_MRS_Manicure', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('BaseColor_Eyebrow_Shadow', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('MultiMask_UVScale_Lip', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('BaseColor_WhiteHair', False, 999, 'INPUT', [0, 0, 0]),
                    MaterialPropertyMetadata('Tex_index_Occlusion_Glove', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Occlusion_Power_Tops', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('map_UVScale_Tattoo_Back', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('MultiMask_B_Power_wrinkle', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Tex_index_Hair', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BaseColor_Tattoo_Right', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Tex_index_Tattoo_Left', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [3.5, 3.5]),
                    MaterialPropertyMetadata('map_UVScale_Normal_Eyebrow', True, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Normal_Power_wrinkle', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('Texture2DArray_Color_WhiteEyebrow', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask_Texture_Lip', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_MultiMask_Stubble', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask_Texture_wrinkle', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Mask_WhiteHair', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Tattoo_Front', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask_Texture_Manicure', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Shoes', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MRS_Texture_Makeup', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Normal_Hair', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Tattoo_Right', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Normal_Scar', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Normal1_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Wear', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Hair', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Tattoo_Back', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Color_Scar', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DamageMask0_MMMM_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Eyebrow_Shadow', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                    MaterialPropertyMetadata('SSSMask_Texture', True, 10, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Normal_Texture_wrinkle', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Color_Makeup', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Normal_Stubble', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Pants', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('BloodTexture_Mask', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MRS_Texture_Manicure', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Maske_Hair', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MRS_Texture_Hair', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_MultiMask_Eyebrow', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Sweat_Textuer', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Tops', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask0_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Normal_Eyebrow', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Emissive0_Texture', False, 15, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Occlusion_Glove', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('OpacityMask0_Texture', False, 11, 'TEXTURE', ''),
                    MaterialPropertyMetadata('BloodTexture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DamageMask0_MMM_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Texture2DArray_Tattoo_Left', False, 999, 'TEXTURE', '')],
    'AVATAR_CLOTH': [MaterialPropertyMetadata('NormalMask_power', True, 29, 'INPUT', 0.8),
                     MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.25),
                     MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.35),
                     MaterialPropertyMetadata('Normal_Power_Detail', True, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('WetnessAbsorbency', True, 79, 'INPUT', 0),
                     MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 1),
                     MaterialPropertyMetadata('map1_UVScale_Detail', True, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [0.5, 0.5, 0.5]),
                     MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('MultiMask_Power_Mark', True, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0.06, 0.0706667, 0.1]),
                     MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                     MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                     MaterialPropertyMetadata('Color15', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color14', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color13', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color12', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0),
                     MaterialPropertyMetadata('Color11', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color10', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('SweatTextureUVScale', True, 110, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('Metallic_Power_Detail', True, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('UVScale_Nomal_Detail', True, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('Tex_index_Mark', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('BloodAmount_B', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('BloodAmount_G', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Select_ColorPower15', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower14', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Effect0Fazz_length1', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('EffectBiass_length1', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Select_ColorPower11', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower10', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower13', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('UVScale_Mark', True, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('Select_ColorPower12', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('DirtAmount_B', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('BloodAmount_R', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('BaseColor_Mark', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [3.5, 3.5]),
                     MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [5.5, 5.7]),
                     MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('BakePreview', False, 999, 'INPUT', 1E-45),
                     MaterialPropertyMetadata('MultiMask_Power16', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MultiMask_Power15', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MultiMask_Power14', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MultiMask_Power13', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MultiMask_Power12', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0.6),
                     MaterialPropertyMetadata('MultiMask_Power11', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('MultiMask_Power10', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Specular_Power_Detail', True, 999, 'INPUT', 0.5),
                     MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                     MaterialPropertyMetadata('Roughness_Power_Detail', True, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                     MaterialPropertyMetadata('MuliMask_Power_Detail', True, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Occlusion_Power_Detail', True, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Color4', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color5', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('MultiMask_Power8', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color6', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('MultiMask_Power9', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color7', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Color1', False, 999, 'INPUT', [0.035, 0.035, 0.035]),
                     MaterialPropertyMetadata('MultiMask_Power4', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color2', False, 999, 'INPUT', [0.01, 0.01, 0.01]),
                     MaterialPropertyMetadata('MultiMask_Power5', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color3', False, 999, 'INPUT', [0.025, 0.025, 0.025]),
                     MaterialPropertyMetadata('MultiMask_Power6', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 4]),
                     MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('MultiMask_Power7', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Select_ColorPower6', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('MultiMask_Power1', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Select_ColorPower7', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('MultiMask_Power2', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color8', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Select_ColorPower4', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('MultiMask_Power3', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Color9', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Select_ColorPower5', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower2', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower3', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower1', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Select_ColorPower8', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 0.98),
                     MaterialPropertyMetadata('Select_ColorPower9', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [5, 5]),
                     MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.6),
                     MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0.1),
                     MaterialPropertyMetadata('SweatColor', False, 999, 'INPUT', [0.56, 0.56, 0.56]),
                     MaterialPropertyMetadata('MultiMask_Texture8', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture9', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture6', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture7', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture4', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture5', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture2', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture3', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture1', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Normal_Texture_Detail', True, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Occlusion_Texture_Detail', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('LayeredMask0_NormalTextures', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                     MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MRS_Texture_Detail', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('DamageMask0_MMMM_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Texture2DArray_Mark', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                     MaterialPropertyMetadata('SweatTexture_Mask', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('BloodTexture_Mask', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture15', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture14', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture16', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture11', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture10', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture13', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask_Texture12', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                     MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Emissive0_Texture', True, 15, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MuliMask_Texture_Detail', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('BaseColor1_Texture', True, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', ''),
                     MaterialPropertyMetadata('BloodTexture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('DamageMask0_MMM_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'AVATAR_HAIR': [MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                    MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MagicDamageMask', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BloodTextureUVScale', True, 93, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('AddNormal_Power', True, 30, 'INPUT', 0.7),
                    MaterialPropertyMetadata('BloodTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('MultiMask_Power2', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power3', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('AlphaThreshold_Shadow', True, 20, 'INPUT', 0),
                    MaterialPropertyMetadata('MultiMask_Power4', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                    MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.25),
                    MaterialPropertyMetadata('DirtAmount_B', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('WetnessAbsorbency', True, 79, 'INPUT', 0),
                    MaterialPropertyMetadata('Emissive0_Color', False, 34, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('DirtTextureColor', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [1, 1, 1]),
                    MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                    MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [0, 0]),
                    MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                    MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('SweatAmount', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                    MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('DirtTextureUVScale', True, 95, 'INPUT', [4, 4]),
                    MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                    MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                    MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('BlendOut', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('NoDither', True, 26, 'INPUT', 0),
                    MaterialPropertyMetadata('MagicDamageAmount', False, 999, 'INPUT', 0.1),
                    MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                    MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                    MaterialPropertyMetadata('hairNoise_strength', True, 60, 'INPUT', 0.3),
                    MaterialPropertyMetadata('AlphaThresholdPrepass', True, 21, 'INPUT', 0.73),
                    MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                    MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                    MaterialPropertyMetadata('Emissive0_Power', False, 35, 'INPUT', 0),
                    MaterialPropertyMetadata('Color2', False, 999, 'INPUT', [0.253165, 0.158118, 0.0705015]),
                    MaterialPropertyMetadata('Color3', False, 999, 'INPUT', [0.16, 0.0384, 0.0384]),
                    MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0.4),
                    MaterialPropertyMetadata('Color1', False, 999, 'INPUT', [0.101266, 0.0599124, 0.0217914]),
                    MaterialPropertyMetadata('Color4', False, 999, 'INPUT', [0, 0, 0]),
                    MaterialPropertyMetadata('MultiMask_Texture4', True, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask_Texture2', True, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MultiMask_Texture3', True, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DirtTexture_Mask', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Occlusion1_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('HairMarschnerNTexture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DirtTexture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_AlphaTextures', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_SpeculerTextures', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('Emissive0_Texture', False, 15, 'TEXTURE', ''),
                    MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', ''),
                    MaterialPropertyMetadata('hairNoiseTexture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('DamageMask0_MMM_Texture', False, 999, 'TEXTURE', ''),
                    MaterialPropertyMetadata('LayeredMask0_BaseColorTextures', False, 999, 'TEXTURE', '')],
    'AVATAR_EYE': [MaterialPropertyMetadata('Emissive0_Power', False, 35, 'INPUT', 0),
                   MaterialPropertyMetadata('IceColorAdd', False, 999, 'INPUT', [0, 0, 0]),
                   MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                   MaterialPropertyMetadata('ShadowBlur', True, 65, 'INPUT', 0.05),
                   MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('SSAO_Blend', False, 67, 'INPUT', 0.5),
                   MaterialPropertyMetadata('refractionDepth', True, 64, 'INPUT', 1),
                   MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 10),
                   MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                   MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                   MaterialPropertyMetadata('BaseColor1', False, 999, 'INPUT', [1.5, 1.5, 1.5]),
                   MaterialPropertyMetadata('BaseColor0', False, 1, 'INPUT', [1.2, 1.2, 1.2]),
                   MaterialPropertyMetadata('BaseColor2', False, 999, 'INPUT', [0.126, 0.104222, 0.092106]),
                   MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                   MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                   MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 4),
                   MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                   MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [0.5, 0.5, 0.5]),
                   MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                   MaterialPropertyMetadata('Hyperemia_Color', True, 61, 'INPUT', [1, 0, 0]),
                   MaterialPropertyMetadata('Hyperemia_Power', True, 62, 'INPUT', 0),
                   MaterialPropertyMetadata('AlphaThreshold', False, 19, 'INPUT', 0),
                   MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [0.5, 0.5]),
                   MaterialPropertyMetadata('MultiMask0_UVScale', True, 999, 'INPUT', [1, 1]),
                   MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                   MaterialPropertyMetadata('MagicDamages_UVScale', True, 101, 'INPUT', [1, 1]),
                   MaterialPropertyMetadata('MultiMask0_Power0', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('IceColorPower', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1.3),
                   MaterialPropertyMetadata('EmissiveScale_Power', False, 999, 'INPUT', 0),
                   MaterialPropertyMetadata('Normal0InnerTexture', True, 21, 'TEXTURE', ''),
                   MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('RefractionMask0', True, 19, 'TEXTURE', ''),
                   MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('Occlusion1_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('EmissiveRedEye_Texture', True, 17, 'TEXTURE', ''),
                   MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                   MaterialPropertyMetadata('IceMask_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('MultiMask0_Texture0', True, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('BaseColorRedEye_Texture', True, 3, 'TEXTURE', ''),
                   MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                   MaterialPropertyMetadata('IceBaseColor_Texture', False, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('OpacityMask0_Texture', False, 11, 'TEXTURE', ''),
                   MaterialPropertyMetadata('MultiMaskRedEye_Texture', True, 999, 'TEXTURE', ''),
                   MaterialPropertyMetadata('BaseColor0Texture', True, 2, 'TEXTURE', '')],
    'AVATAR_GLASS': [MaterialPropertyMetadata('BaseColor1', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('BaseColor0', False, 1, 'INPUT', [0.588785, 0.588785, 0.588785]),
                     MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                     MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 0.0444444),
                     MaterialPropertyMetadata('StoneMap_UVScale', True, 109, 'INPUT', [4, 4]),
                     MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 0.5),
                     MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0),
                     MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Normal1_Power', True, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('StoneColor', False, 999, 'INPUT', [1, 1, 1]),
                     MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                     MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                     MaterialPropertyMetadata('Transparency_biass', True, 24, 'INPUT', 0.5),
                     MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                     MaterialPropertyMetadata('Transparency0_Power', True, 23, 'INPUT', 1),
                     MaterialPropertyMetadata('StoneNormal_Power', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                     MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                     MaterialPropertyMetadata('Transparency_exp', True, 25, 'INPUT', 0.25),
                     MaterialPropertyMetadata('Stone_Power', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                     MaterialPropertyMetadata('StoneColor_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Normal1_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('StoneNormal_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Occlusion1_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('StoneMask_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                     MaterialPropertyMetadata('MultiMask0_Texture', False, 999, 'TEXTURE', ''),
                     MaterialPropertyMetadata('Transparency0_Texture', True, 12, 'TEXTURE', '')],
    'BASIC_MATERIAL': [MaterialPropertyMetadata('AlphaThreshold', True, 19, 'INPUT', 0),
                       MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 0.92),
                       MaterialPropertyMetadata('Normal1_Power', True, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                       MaterialPropertyMetadata('Occlusion1_Power', True, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('WetnessScale', True, 81, 'INPUT', 0.95),
                       MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                       MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                       MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                       MaterialPropertyMetadata('BaseColor1', True, 999, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('WetnessAbsorbency', True, 79, 'INPUT', 0.4),
                       MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                       MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 1),
                       MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                       MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                       MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                       MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Emissive0_Texture', True, 15, 'TEXTURE', ''),
                       MaterialPropertyMetadata('OpacityMask0_Texture', True, 11, 'TEXTURE', '')],
    'GLASS_MATERIAL': [MaterialPropertyMetadata('EffectBiass_length', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('mapLM_UVScale', False, 999, 'INPUT', [1, 1]),
                       MaterialPropertyMetadata('Emissive0_Power', True, 35, 'INPUT', 0),
                       MaterialPropertyMetadata('Light_EV', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Emissive0_Color', True, 34, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('Effect0_Power', False, 999, 'INPUT', [0, 0, 0]),
                       MaterialPropertyMetadata('Effect0Fazz_length', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Fade_Power', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('Specular0_Power', True, 17, 'INPUT', 1),
                       MaterialPropertyMetadata('BaseColor1', False, 999, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('Transparency_exp', True, 25, 'INPUT', 0),
                       MaterialPropertyMetadata('BaseColor0', True, 1, 'INPUT', [1, 1, 1]),
                       MaterialPropertyMetadata('Occlusion1_Power', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('map1_UVScale', False, 999, 'INPUT', [1, 1]),
                       MaterialPropertyMetadata('Transparency_biass', True, 24, 'INPUT', 0.5),
                       MaterialPropertyMetadata('Normal0_Power', True, 27, 'INPUT', 1),
                       MaterialPropertyMetadata('Transparency0_Power', True, 23, 'INPUT', 1),
                       MaterialPropertyMetadata('Occlusion0_Power', True, 3, 'INPUT', 0.7),
                       MaterialPropertyMetadata('Normal1_Power', False, 999, 'INPUT', 1),
                       MaterialPropertyMetadata('Roughness0_Power', True, 15, 'INPUT', 1),
                       MaterialPropertyMetadata('MultiMask0_Power', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('Metallic0_Power', True, 12, 'INPUT', 1),
                       MaterialPropertyMetadata('Emissive_EV', False, 999, 'INPUT', 0),
                       MaterialPropertyMetadata('MRS0_Texture', True, 6, 'TEXTURE', ''),
                       MaterialPropertyMetadata('BaseColor0_Texture', True, 1, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Occlusion0_Texture', True, 4, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Normal0_Texture', True, 20, 'TEXTURE', ''),
                       MaterialPropertyMetadata('Transparency0_Texture', True, 12, 'TEXTURE', '')]
}


class FlagrumMaterialProperty(PropertyGroup):
    property_name: StringProperty()
    is_relevant: BoolProperty()
    importance: IntProperty()
    property_type: StringProperty()


class FlagrumMaterialPropertyCollection(PropertyGroup):
    material_id: StringProperty()
    property_collection: CollectionProperty(type=FlagrumMaterialProperty)

    AddNormal_Power: FloatProperty(default=0.7)
    AlphaThreshold: FloatProperty(default=0)
    BaseColor0: FloatVectorProperty(size=3, default=[1.5, 1.5, 1.5])
    BloodTextureUVScale: FloatVectorProperty(size=2, default=[5, 5])
    DirtTextureUVScale: FloatVectorProperty(size=2, default=[6, 6])
    Emissive0_Color: FloatVectorProperty(size=3, default=[1, 1, 1])
    Emissive0_Power: FloatProperty(default=0)
    MagicDamages_UVScale: FloatVectorProperty(size=2, default=[7, 8])
    Metallic0_Power: FloatProperty(default=0)
    Normal0_Power: FloatProperty(default=1)
    NormalMask_power: FloatProperty(default=0.8)
    Occlusion0_Power: FloatProperty(default=0.7)
    Occlusion1_Power: FloatProperty(default=1)
    Roughness0_Power: FloatProperty(default=1)
    Specular0_Power: FloatProperty(default=0.3)
    StoneMap_UVScale: FloatVectorProperty(size=2, default=[4, 4])
    SweatTextureUVScale: FloatVectorProperty(size=2, default=[1, 1])
    AlphaAllFadeInEndDistance: FloatProperty(default=0.13)
    AlphaAllFadeInStartDistance: FloatProperty(default=0.1)
    BloodAmount_B: FloatProperty(default=0)
    BloodAmount_G: FloatProperty(default=0)
    BloodAmount_R: FloatProperty(default=0)
    BloodTextureColor: FloatVectorProperty(size=3, default=[1.5, 1.5, 1.5])
    DirtAmount_B: FloatProperty(default=0)
    DirtTextureColor: FloatVectorProperty(size=3, default=[0.7, 0.7, 0.7])
    Effect0_Power: FloatVectorProperty(size=3, default=[0, 0, 0])
    Effect0Fazz_length: FloatProperty(default=10)
    EffectBiass_length: FloatProperty(default=1.3)
    Emissive_EV: FloatProperty(default=0)
    Fade_Power: FloatProperty(default=1)
    Fazz0_length: FloatProperty(default=0.5)
    FazzMask0_Power: FloatProperty(default=0)
    FP_FadePower: FloatProperty(default=0)
    IceColorAdd: FloatVectorProperty(size=3, default=[0.024, 0.0281347, 0.04])
    IceColorPower: FloatProperty(default=0.5)
    Light_EV: FloatProperty(default=0)
    MagicDamageAmount: FloatProperty(default=0.35)
    MagicDamageMask: FloatProperty(default=0.3)
    map1_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    mapLM_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    occlusion_set: FloatProperty(default=0)
    Stone_Power: FloatProperty(default=0)
    StoneColor: FloatVectorProperty(size=3, default=[1, 1, 1])
    StoneNormal_Power: FloatProperty(default=1)
    SweatAmount: FloatProperty(default=0)
    SweatColor: FloatVectorProperty(size=3, default=[0.5, 0.5, 0.5])
    aoSaturation_Power: FloatProperty(default=2)
    BaseColor1: FloatVectorProperty(size=3, default=[1, 1, 1])
    Normal1_Power: FloatProperty(default=1)
    SkinBloodAmount_A: FloatProperty(default=0)
    SkinBloodAmount_B: FloatProperty(default=0)
    SkinBloodAmount_G: FloatProperty(default=0)
    SkinBloodAmount_R: FloatProperty(default=0)
    SSS_HueColor: FloatVectorProperty(size=3, default=[1, 0, 0])
    SSSStrength: FloatProperty(default=1)
    VertexColor_MaskControl: FloatProperty(default=0.35)
    WetnessAbsorbency: FloatProperty(default=0)
    WetnessScale: FloatProperty(default=0.25)
    map1_UVScale0: FloatVectorProperty(size=2, default=[1, 1])
    MultiMask0_Power: FloatProperty(default=0)
    SkinDirtAmount_B: FloatProperty(default=0)
    SSAO_Blend: FloatProperty(default=0.5)
    ClothDamageAmount_B: FloatProperty(default=0)
    ClothDamageAmount_G: FloatProperty(default=0)
    ClothDamageAmount_R: FloatProperty(default=0)
    ClothDamageColor: FloatVectorProperty(size=3, default=[1, 1, 1])
    NormalMask_Power: FloatProperty(default=0.3)
    ClothDamageColor_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    param0: FloatVectorProperty(size=2, default=[1, 1])
    AlphaThreshold_Shadow: FloatProperty(default=0.1)
    AlphaThresholdPrepass: FloatProperty(default=0.73)
    BlendOut: FloatProperty(default=0.3)
    hairNoise_strength: FloatProperty(default=0.3)
    NoDither: FloatProperty(default=0)
    EmissiveScale_Power: FloatProperty(default=1)
    Hyperemia_Color: FloatVectorProperty(size=3, default=[1, 1, 1])
    Hyperemia_Power: FloatProperty(default=0)
    refractionDepth: FloatProperty(default=1)
    ShadowBlur: FloatProperty(default=0.05)
    Curvature_Bias0: FloatProperty(default=0.5)
    Detail_UVScale: FloatVectorProperty(size=2, default=[25, 25])
    MultiMask2_Power: FloatProperty(default=0)
    MultiMask3_Power: FloatProperty(default=0)
    MultiMask4_Power: FloatProperty(default=0)
    ShadowBoundary_Saturation: FloatProperty(default=0.5)
    Specular1_Power: FloatProperty(default=0.3)
    color_set: FloatProperty(default=0)
    SSS_normalMipBias: FloatProperty(default=2)
    MultiMask1_Power: FloatProperty(default=1)
    Normal1_UVScale: FloatVectorProperty(size=2, default=[15, 15])
    BaseColor_Eyebrow: FloatVectorProperty(size=3, default=[0.094, 0.083772, 0.061382])
    BaseColor_Hair: FloatVectorProperty(size=3, default=[0.00585946, 0.00585946, 0.00585946])
    BaseColor_Tattoo_Back: FloatVectorProperty(size=3, default=[1, 1, 1])
    DefaultLipColor: FloatVectorProperty(size=3, default=[0.69, 0.4209, 0.4209])
    DefaultSkinColor: FloatVectorProperty(size=3, default=[0.69, 0.4209, 0.4209])
    map_UVScale_Color_Scar: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Eyebrow_Shadow: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Normal_Eyebrow: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Normal_Scar: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Tattoo_Back: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Tattoo_Left: FloatVectorProperty(size=2, default=[1, 1])
    map_UVScale_Tattoo_Right: FloatVectorProperty(size=2, default=[1, 1])
    map1_UVScale_Makeup: FloatVectorProperty(size=2, default=[1, 1])
    map1_UVScale2_MRS_Manicure: FloatVectorProperty(size=2, default=[1, 1])
    Metallic_Power_Hair: FloatProperty(default=0)
    Metallic_Power_Makeup: FloatProperty(default=1)
    MultiMask_Power_Lip: FloatProperty(default=1)
    MultiMask_Power_Manicure: FloatProperty(default=0)
    MultiMask_UVScale_Hair: FloatVectorProperty(size=2, default=[1, 1])
    MultiMask_UVScale_Lip: FloatVectorProperty(size=2, default=[1, 1])
    MultiMask_UVScale_Stubble: FloatVectorProperty(size=2, default=[1, 1])
    Normal_UVScale_Hair: FloatVectorProperty(size=2, default=[1, 1])
    Normal_UVScale_Stubble: FloatVectorProperty(size=2, default=[1, 1])
    Roughness_Power_Hair: FloatProperty(default=1)
    Roughness_Power_Makeup: FloatProperty(default=1)
    Roughness_Power_Manicure: FloatProperty(default=1)
    Specular_Power_Hair: FloatProperty(default=1)
    Specular_Power_Makeup: FloatProperty(default=0.5)
    Specular_Power_Manicure: FloatProperty(default=0.5)
    UVScale_Color_WhiteEyebrow: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_Color_WhiteHair: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_Makeup: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_Makeup0: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_MultiMask_Eyebrow: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_MultiMask_Manicure: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_Tattoo_Front: FloatVectorProperty(size=2, default=[1, 1])
    BakePreview: FloatProperty(default=1E-45)
    BaseColor_Eyebrow_Shadow: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Makeup: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Manicure: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Scar: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Stubble: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Tattoo_Front: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Tattoo_Left: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_Tattoo_Right: FloatVectorProperty(size=3, default=[1, 1, 1])
    BaseColor_WhiteEyebrow: FloatVectorProperty(size=3, default=[0, 0, 0])
    BaseColor_WhiteHair: FloatVectorProperty(size=3, default=[0, 0, 0])
    LipColor: FloatVectorProperty(size=3, default=[0.69, 0.4209, 0.4209])
    Metallic_Power_Manicure: FloatProperty(default=0)
    MultiMask_B_Power_wrinkle: FloatProperty(default=0)
    MultiMask_G_Power_wrinkle: FloatProperty(default=0)
    MultiMask_Power_Eyebrow: FloatProperty(default=1)
    MultiMask_Power_Hair: FloatProperty(default=1)
    MultiMask_Power_Makeup: FloatProperty(default=1)
    MultiMask_Power_Scar: FloatProperty(default=1)
    MultiMask_Power_Stubble: FloatProperty(default=0)
    MultiMask_Power_Tattoo_Back: FloatProperty(default=0)
    MultiMask_Power_Tattoo_Front: FloatProperty(default=1)
    MultiMask_Power_Tattoo_Left: FloatProperty(default=0)
    MultiMask_Power_Tattoo_Right: FloatProperty(default=0)
    MultiMask_Power_WhiteEyebrow: FloatProperty(default=1)
    MultiMask_Power_WhiteHair: FloatProperty(default=1)
    MultiMask_R_Power_wrinkle: FloatProperty(default=0)
    Normal_Power_wrinkle: FloatProperty(default=1)
    Occlusion_Power_Glove: FloatProperty(default=1)
    Occlusion_Power_Hair: FloatProperty(default=1)
    Occlusion_Power_Pants: FloatProperty(default=1)
    Occlusion_Power_Shoes: FloatProperty(default=1)
    Occlusion_Power_Tops: FloatProperty(default=1)
    Occlusion_Power_Wear: FloatProperty(default=1)
    SkinColor: FloatVectorProperty(size=3, default=[0.69, 0.4209, 0.4209])
    Tex_index_Eyebrow: FloatProperty(default=0)
    Tex_index_Hair: FloatProperty(default=0)
    Tex_index_Makeup: FloatProperty(default=0)
    Tex_index_Occlusion_Glove: FloatProperty(default=0)
    Tex_index_Occlusion_Pants: FloatProperty(default=0)
    Tex_index_Occlusion_Shoes: FloatProperty(default=0)
    Tex_index_Occlusion_Tops: FloatProperty(default=0)
    Tex_index_Occlusion_Wear: FloatProperty(default=0)
    Tex_index_Scar: FloatProperty(default=0)
    Tex_index_Stubble: FloatProperty(default=0)
    Tex_index_Tattoo_Back: FloatProperty(default=0)
    Tex_index_Tattoo_Front: FloatProperty(default=0)
    Tex_index_Tattoo_Left: FloatProperty(default=0)
    Tex_index_Tattoo_Right: FloatProperty(default=0)
    map1_UVScale_Detail: FloatVectorProperty(size=2, default=[1, 1])
    Metallic_Power_Detail: FloatProperty(default=0)
    MuliMask_Power_Detail: FloatProperty(default=0)
    MultiMask_Power_Mark: FloatProperty(default=0)
    Normal_Power_Detail: FloatProperty(default=1)
    Occlusion_Power_Detail: FloatProperty(default=1)
    Roughness_Power_Detail: FloatProperty(default=1)
    Specular_Power_Detail: FloatProperty(default=0.5)
    UVScale_Mark: FloatVectorProperty(size=2, default=[1, 1])
    UVScale_Nomal_Detail: FloatVectorProperty(size=2, default=[1, 1])
    BaseColor_Mark: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color1: FloatVectorProperty(size=3, default=[0.035, 0.035, 0.035])
    Color10: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color11: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color12: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color13: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color14: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color15: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color2: FloatVectorProperty(size=3, default=[0.01, 0.01, 0.01])
    Color3: FloatVectorProperty(size=3, default=[0.025, 0.025, 0.025])
    Color4: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color5: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color6: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color7: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color8: FloatVectorProperty(size=3, default=[1, 1, 1])
    Color9: FloatVectorProperty(size=3, default=[1, 1, 1])
    Effect0Fazz_length1: FloatProperty(default=0)
    EffectBiass_length1: FloatProperty(default=1)
    MultiMask_Power1: FloatProperty(default=1)
    MultiMask_Power10: FloatProperty(default=0)
    MultiMask_Power11: FloatProperty(default=0)
    MultiMask_Power12: FloatProperty(default=0)
    MultiMask_Power13: FloatProperty(default=0)
    MultiMask_Power14: FloatProperty(default=0)
    MultiMask_Power15: FloatProperty(default=0)
    MultiMask_Power16: FloatProperty(default=1)
    MultiMask_Power2: FloatProperty(default=1)
    MultiMask_Power3: FloatProperty(default=1)
    MultiMask_Power4: FloatProperty(default=0)
    MultiMask_Power5: FloatProperty(default=0)
    MultiMask_Power6: FloatProperty(default=0)
    MultiMask_Power7: FloatProperty(default=0)
    MultiMask_Power8: FloatProperty(default=0)
    MultiMask_Power9: FloatProperty(default=0)
    Select_ColorPower1: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower10: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower11: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower12: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower13: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower14: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower15: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower2: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower3: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower4: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower5: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower6: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower7: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower8: FloatVectorProperty(size=3, default=[0, 0, 0])
    Select_ColorPower9: FloatVectorProperty(size=3, default=[0, 0, 0])
    Tex_index_Mark: FloatProperty(default=0)
    MultiMask0_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    BaseColor2: FloatVectorProperty(size=3, default=[0.126, 0.104222, 0.092106])
    MultiMask0_Power0: FloatProperty(default=1)
    Transparency_biass: FloatProperty(default=0.5)
    Transparency_exp: FloatProperty(default=0.25)
    Transparency0_Power: FloatProperty(default=1)
    BaseColor0_hue: FloatProperty(default=0.5)
    BaseColor0_saturation: FloatProperty(default=0.5)
    BaseColor0_value: FloatProperty(default=0.5)
    BaseColor1_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    BaseColor2_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    BaseColor3_UVScale0: FloatVectorProperty(size=2, default=[1, 1])
    LayerdMask_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    Metallic1_Power: FloatProperty(default=0)
    Metallic2_Power0: FloatProperty(default=0)
    MRS2_UVScale0: FloatVectorProperty(size=2, default=[1, 1])
    Normal1_Power0: FloatProperty(default=1)
    Normal1_UVScale0: FloatVectorProperty(size=2, default=[1, 1])
    Normal2_Power: FloatProperty(default=1)
    Normal2_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    Occlusion1_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    Roughness1_Power: FloatProperty(default=1)
    Roughness2_Power0: FloatProperty(default=1)
    Specular1_Power0: FloatProperty(default=0)
    Tex_index: FloatProperty(default=0)
    BaseColor3: FloatVectorProperty(size=3, default=[1, 1, 1])
    Fading_Power: FloatProperty(default=0)
    Fading_Power0: FloatProperty(default=0)
    FadingField_Power: FloatProperty(default=0)
    FadingField_Power0: FloatProperty(default=0)
    FadingTone: FloatProperty(default=0)
    FadingTone0: FloatProperty(default=0)
    map1_UVScale1: FloatVectorProperty(size=2, default=[1, 1])
    map4_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    mm1_B_Power: FloatProperty(default=1)
    mm1_G_Power: FloatProperty(default=1)
    mm1_R_Power: FloatProperty(default=1)
    mm2_B_Power: FloatProperty(default=1)
    mm2_G_Power: FloatProperty(default=1)
    mm2_R_Power: FloatProperty(default=1)
    MRS1_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    aoSaturation_Blend: FloatProperty(default=1)
    map3_UVScale0: FloatVectorProperty(size=2, default=[1, 1])
    MultiMask3_Power0: FloatProperty(default=0)
    FresnelFade_Roughness: FloatProperty(default=0)
    Metallic0Power: FloatProperty(default=1)
    Normal0Power: FloatProperty(default=1)
    Occlusion0Power: FloatProperty(default=1)
    Roughness0Power: FloatProperty(default=1)
    Specular0Power: FloatProperty(default=0.5)
    UVScale: FloatVectorProperty(size=2, default=[1, 1])
    WetnessCrackDepth: FloatProperty(default=0.1)
    LayeredMask0_MaskLvPw0: FloatProperty(default=1)
    LayeredMask0_MaskLvPw1: FloatProperty(default=1)
    LayeredMask0_MaskLvPw2: FloatProperty(default=1)
    LayeredMask0_UVScale: FloatVectorProperty(size=2, default=[5, 5])
    Transparency0: FloatProperty(default=1)
    AutoEmissivePower: FloatProperty(default=1)
    EdgeEmissivePower: FloatProperty(default=1)
    EmissiveColor0: FloatVectorProperty(size=3, default=[1, 1, 1])
    EmissiveGrowRange: FloatProperty(default=8)
    EmissiveGrowRate: FloatProperty(default=0)
    FadeOutEnd: FloatProperty(default=80)
    FadeOutRate: FloatProperty(default=16)
    FadeOutStart: FloatProperty(default=35)
    FadeMinScale: FloatProperty(default=0)
    BodyColorMask_Power: FloatProperty(default=1)
    mapDamage_UVScale: FloatVectorProperty(size=2, default=[3, 3])
    mapRust_UVScale: FloatVectorProperty(size=2, default=[10, 10])
    Normal_UVScale: FloatVectorProperty(size=2, default=[1, 1])
    Rust_Power: FloatProperty(default=0)
    RustNormal_Power: FloatProperty(default=1)
    Wetness_Absorbency: FloatProperty(default=0)
    Wetness_CrackDepth: FloatProperty(default=0.1)
    Wetness_Flow_Speed: FloatProperty(default=1)
    Wetness_Flow_Strength: FloatProperty(default=2)
    Wetness_Flow_Stretch: FloatProperty(default=1)
    Wetness_Flow_TopsurfaceStrength: FloatProperty(default=200)
    Wetness_Flow_WorldScale: FloatProperty(default=2)
    Wetness_Ripple_WorldScale: FloatProperty(default=8)
    Wetness_Scale: FloatProperty(default=1)
    bodycolor0: FloatVectorProperty(size=3, default=[0.66, 0.8, 0.01])
    bodycolor1: FloatVectorProperty(size=3, default=[0.125, 2, 0.7])
    Damage_Power: FloatProperty(default=1)
    DamageColorMaskPower: FloatProperty(default=1)
    DamageColorPram: FloatVectorProperty(size=3, default=[1, 1, 1])
    DamageField_Power: FloatProperty(default=0)
    FadingColorSaturation: FloatProperty(default=0.8)
    FadingColorValue: FloatProperty(default=0.5)
    MaskTex_nm1_B: FloatProperty(default=0)
    MaskTex_nm1_G: FloatProperty(default=0)
    MaskTex_nm1_R: FloatProperty(default=0)
    MaskTex_nm2_B: FloatProperty(default=0)
    MaskTex_nm2_G: FloatProperty(default=0)
    MaskTex_nm2_R: FloatProperty(default=0)
    MaskTex_nm3_G: FloatProperty(default=0)
    MaskTex_nm3_R: FloatProperty(default=0)
    NormalDamage_Power: FloatProperty(default=1)
    RustField_Power: FloatProperty(default=1)
    RustTone: FloatProperty(default=0.1)
    Wetness_Ripple_Strength: FloatProperty(default=1)
    emissive0: FloatProperty(default=0)
    emissive1: FloatProperty(default=0)
    emissive2: FloatProperty(default=0)
    emissive3: FloatProperty(default=0)
    emissive4: FloatProperty(default=0)
    emissive5: FloatProperty(default=0)
    DamagePower: FloatProperty(default=0)
    Emissive_EV0: FloatProperty(default=0)
    Emissive_EV1: FloatProperty(default=0)
    Emissive_EV2: FloatProperty(default=0)
    Emissive_EV3: FloatProperty(default=0)
    Emissive1_Color: FloatVectorProperty(size=3, default=[1, 1, 1])
    Emissive1_Power: FloatProperty(default=0)
    Emissive2_Color: FloatVectorProperty(size=3, default=[1, 1, 1])
    Emissive2_Power: FloatProperty(default=0)
    Emissive3_Color: FloatVectorProperty(size=3, default=[1, 1, 1])
    Emissive3_Power: FloatProperty(default=0)
    Light_EV0: FloatProperty(default=0)
    Light_EV1: FloatProperty(default=0)
    Light_EV2: FloatProperty(default=0)
    Light_EV3: FloatProperty(default=0)
    fresnel_bias: FloatProperty(default=0)
    fresnel_bias0: FloatProperty(default=0)
    fresnel_exp: FloatProperty(default=0)
    fresnel_exp0: FloatProperty(default=0)
    fresnel_nr: FloatProperty(default=0)
    fresnel_nr0: FloatProperty(default=0)
    fresnelColor: FloatVectorProperty(size=3, default=[1, 1, 1])
    BasePaint_ColorFront: FloatVectorProperty(size=3, default=[0.117, 0.089739, 0.089739])
    BasePaint_ColorSide: FloatVectorProperty(size=3, default=[0.098, 0.098, 0.098])
    BasePaint_Metallic: FloatProperty(default=0.5)
    BasePaint_Roughness: FloatProperty(default=0.15)
    ClearCoat_Visibility: FloatProperty(default=0.6)
    Drops_Speed: FloatProperty(default=0.25)
    DustColMask_UVScale: FloatVectorProperty(size=2, default=[40, 40])
    mapDust_UVScale: FloatVectorProperty(size=2, default=[20, 20])
    mapMud_UVScale: FloatVectorProperty(size=2, default=[50, 50])
    metalflake_color_all: FloatVectorProperty(size=3, default=[1, 1, 1])
    MetalFlake_ColorFront: FloatVectorProperty(size=3, default=[0.898, 0.4041, 0.4041])
    MetalFlake_ColorSide: FloatVectorProperty(size=3, default=[0.43485, 0.575773, 0.65])
    MetalFlake_Density: FloatProperty(default=0.461)
    Occlusion0: FloatProperty(default=1)
    RainDrop_uvScale: FloatProperty(default=6)
    Scratch1_UVScale: FloatVectorProperty(size=2, default=[20, 20])
    Sprinkles_UVscale: FloatVectorProperty(size=2, default=[3, 3])
    Drops_Coverage: FloatProperty(default=1)
    Drops_Size: FloatProperty(default=0.25)
    Drops_Tail: FloatProperty(default=1)
    Dust_Power: FloatProperty(default=0)
    DustColorParam: FloatVectorProperty(size=3, default=[0.35, 0.219333, 0.105])
    DustTone: FloatProperty(default=0.6)
    Mud_Power: FloatProperty(default=0)
    MudColorPram: FloatVectorProperty(size=3, default=[0.15, 0.15, 0.15])
    MudNormal_Power: FloatProperty(default=0.3)
    Scratch1_Power: FloatProperty(default=0)
    Scratch1Normal_Power: FloatProperty(default=1)
    Scratch1Tone: FloatProperty(default=0.43)
    Scratch2_Power: FloatProperty(default=0)
    Scratch2_UVScale: FloatVectorProperty(size=2, default=[5, 6])
    Scratch2ColMaskPow: FloatProperty(default=1)
    Scratch2Normal_Power: FloatProperty(default=1)
    Scratch2Tone: FloatProperty(default=0.43)
    SpinklesNormal_Strength: FloatProperty(default=0.3)
    Stain_Power: FloatProperty(default=0)
    StainTone: FloatProperty(default=1)
    Sticker1_Pivot: FloatVectorProperty(size=2, default=[0.5, 0.5])
    Sticker1_Rotate: FloatProperty(default=0)
    Sticker1_Scale: FloatVectorProperty(size=2, default=[1, 4])
    Sticker1_UVTrans: FloatVectorProperty(size=2, default=[0, -0.125])
    Sticker2_Pivot: FloatVectorProperty(size=2, default=[0.5, 0.5])
    Sticker2_Rotate: FloatProperty(default=0)
    Sticker2_Scale: FloatVectorProperty(size=2, default=[1, 4])
    Sticker2_UVTrans: FloatVectorProperty(size=2, default=[0.037, 0.151])
    Sticker3_Pivot: FloatVectorProperty(size=2, default=[0.5, 0.5])
    Sticker3_Rotate: FloatProperty(default=0)
    Sticker3_Scale: FloatVectorProperty(size=2, default=[2, 8])
    Sticker3_UVTrans: FloatVectorProperty(size=2, default=[-0.25, 0.437])
    Sticker4_Pivot: FloatVectorProperty(size=2, default=[0.5, 0.5])
    Sticker4_Rotate: FloatProperty(default=0)
    Sticker4_Scale: FloatVectorProperty(size=2, default=[2, 8])
    Sticker4_UVTrans: FloatVectorProperty(size=2, default=[0.25, 0.437])
    Sticker5_Pivot: FloatVectorProperty(size=2, default=[0.5, 0.5])
    Sticker5_Rotate: FloatProperty(default=0)
    Sticker5_Scale: FloatVectorProperty(size=2, default=[1, 4])
    Sticker5_UVTrans: FloatVectorProperty(size=2, default=[0, -0.374])
    UV_Rotate: FloatProperty(default=0)
    emissive6: FloatProperty(default=0)
    emissive7: FloatProperty(default=0)
    emissive8: FloatProperty(default=0)
    emissive9: FloatProperty(default=0)
    Distortion_amount: FloatProperty(default=0.7)
    DustPower: FloatProperty(default=0)
    DustTile_UVScale: FloatVectorProperty(size=2, default=[10, 10])
    DustTileColor: FloatVectorProperty(size=3, default=[0.5, 0.354167, 0.25])
    Fade_Power0: FloatProperty(default=1)
    glass_crush_UVScale: FloatVectorProperty(size=2, default=[2.5, 2.5])
    GlassCrashSpeclParam: FloatProperty(default=0.5)
    GlassDamagePower: FloatProperty(default=0)
    spinklesNormal_Strength: FloatProperty(default=0.05)
    sprinkles_UVscale: FloatVectorProperty(size=2, default=[3, 3])
    wipers_L_pause_time: FloatProperty(default=1.75)
    wipers_LtoR_time: FloatProperty(default=0.7)
    wipers_On: FloatProperty(default=0)
    wipers_R_pause_time: FloatProperty(default=0)
    wipers_RtoL_time: FloatProperty(default=0.45)
    wipers_startOffset: FloatProperty(default=0)
    DetailMask0_Power: FloatProperty(default=1)
    mapDetail_UVScale: FloatVectorProperty(size=2, default=[200, 200])
    Color0_param: FloatVectorProperty(size=3, default=[0.421, 0.321499, 0.167979])
    Color1_param: FloatVectorProperty(size=3, default=[0.254902, 0.165049, 0.0949635])
    Color1PatternPower: FloatProperty(default=35)
    Color2_param: FloatVectorProperty(size=3, default=[0.0186923, 0, 0])
    Color3_param: FloatVectorProperty(size=3, default=[1, 1, 1])
    Metal0_param: FloatProperty(default=0.5)
    Roughness0_param: FloatProperty(default=0.5)
    BDust_Power: FloatProperty(default=0)
    BDustTone: FloatProperty(default=0.5)
    BrakeDustColorParam: FloatVectorProperty(size=3, default=[0.028764, 0.0347059, 0.047])
    Metal1_param: FloatProperty(default=0.5)
    Metal2_param: FloatProperty(default=0.5)
    MudColorParam: FloatVectorProperty(size=3, default=[0.08, 0.08, 0.08])
    Roughness1_param: FloatProperty(default=0.5)
    Roughness2_param: FloatProperty(default=0.5)
    colorizeMode: FloatProperty(default=0)
    diffuseBlend: FloatProperty(default=0)
    glintStrength: FloatProperty(default=0)
    receiveShadows: FloatProperty(default=0)
    rootAlphaFalloff: FloatProperty(default=0)
    rootColor: FloatVectorProperty(size=4, default=[0, 0, 0, 0])
    rootTipColorFalloff: FloatProperty(default=0)
    rootTipColorWeight: FloatProperty(default=0)
    specularColor: FloatVectorProperty(size=4, default=[0, 0, 0, 0])
    specularNoiseScale: FloatProperty(default=0)
    specularPrimaryPower: FloatProperty(default=0)
    tipColor: FloatVectorProperty(size=4, default=[0, 0, 0, 0])
    useRootColorTexture: FloatProperty(default=0)
    useSpecularTexture: FloatProperty(default=0)
    useStrandTexture: FloatProperty(default=0)
    useTipColorTexture: FloatProperty(default=0)
    # __reserved__: FloatProperty(default=0)
    # __reservedLOD___: FloatProperty(default=0)
    # __shadowReserved1__: FloatProperty(default=0)
    # __shadowReserved2__: FloatProperty(default=0)
    # _diffuseUnused_: FloatProperty(default=0)
    # _specularUnused_: FloatProperty(default=0)
    camPosition: FloatVectorProperty(size=4, default=[-107374180, -107374180, 1.1E-43, 0])
    diffuseHairNormalWeight: FloatProperty(default=0)
    diffuseScale: FloatProperty(default=0)
    glintCount: FloatProperty(default=0)
    glintExponent: FloatProperty(default=0)
    inverseProjection: FloatVectorProperty(size=16,
                                           default=[-107374180, -107374180, 1.18E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                    3.9234877E-38, 0])
    inverseViewport: FloatVectorProperty(size=16,
                                         default=[-107374180, -107374180, 1.15E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                  3.92425E-38, 0])
    inverseViewProjection: FloatVectorProperty(size=16,
                                               default=[0.5, 0.5, 0.5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.923443E-38,
                                                        0])
    inverseViewProjectionViewport: FloatVectorProperty(size=16,
                                                       default=[-107374180, -107374180, 1.35E-43, 0, 0, 0, 0, 0, 0, 0,
                                                                0, 0, 0, 0, 3.924295E-38, 0])
    lodAlphaFactor: FloatProperty(default=0)
    lodDetailFactor: FloatProperty(default=0)
    lodDistanceFactor: FloatProperty(default=0)
    modelCenter: FloatVectorProperty(size=4, default=[0, 0, 0, 0])
    noiseTable: FloatVectorProperty(size=4, default=[0, 0, 0, 0])
    prevViewport: FloatVectorProperty(size=16,
                                      default=[-107374180, -107374180, 1.11E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                               3.267541E-38, 0])
    prevViewProjection: FloatVectorProperty(size=16,
                                            default=[-107374180, -107374180, 1.19E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                     3.2675096E-38, 0])
    shadowSigma: FloatProperty(default=0)
    shadowUseLeftHanded: FloatProperty(default=0)
    specularPrimaryBreakup: FloatProperty(default=0)
    specularPrimaryScale: FloatProperty(default=0)
    specularSecondaryOffset: FloatProperty(default=0)
    specularSecondaryPower: FloatProperty(default=0)
    specularSecondaryScale: FloatProperty(default=0)
    strandBlendMode: FloatProperty(default=0)
    strandBlendScale: FloatProperty(default=0)
    strandPointCount: FloatProperty(default=0)
    viewport: FloatVectorProperty(size=16, default=[-107374180, -107374180, 1.05E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                    3.9243397E-38, 0])
    viewProjection: FloatVectorProperty(size=16,
                                        default=[-107374180, -107374180, 1.14E-43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                 3.2674783E-38, 0])
    BaseColor0_saturation0: FloatProperty(default=0.5)
    Fazz_EV: FloatProperty(default=4.5)
    EdgeLight_Color: FloatVectorProperty(size=3, default=[0, 0.224659, 1])
    EdgeLight_length: FloatProperty(default=-0.1)
    EdgeLight_Power: FloatProperty(default=0.3)
    Emissivemap_UVScale: FloatVectorProperty(size=2, default=[3, 3])
    EmissiveMask_UVScale: FloatVectorProperty(size=2, default=[0, 0])
    BaseColor0_Texture: StringProperty()
    MRS0_Texture: StringProperty()
    Normal0_Texture: StringProperty()
    Occlusion0_Texture: StringProperty()
    OpacityMask0_Texture: StringProperty()
    BloodTexture: StringProperty()
    BloodTexture_Mask: StringProperty()
    DirtTexture: StringProperty()
    DirtTexture_Mask: StringProperty()
    IceBaseColor_Texture: StringProperty()
    IceMask_Texture: StringProperty()
    LayeredMask0_AlphaTextures: StringProperty()
    LayeredMask0_BaseColorTextures: StringProperty()
    LayeredMask0_NormalTextures: StringProperty()
    LayeredMask0_SpeculerTextures: StringProperty()
    StoneColor_Texture: StringProperty()
    StoneNormal_Texture: StringProperty()
    SweatTexture_Mask: StringProperty()
    SSSMask_Texture: StringProperty()
    BaseColor1_Texture: StringProperty()
    MultiMask0_Texture: StringProperty()
    StoneMask_Texture: StringProperty()
    Sweat_Textuer: StringProperty()
    ClothDamageColor_Texture: StringProperty()
    OpacityMask1_Texture: StringProperty()
    HairMarschnerNTexture: StringProperty()
    hairNoiseTexture: StringProperty()
    BaseColor0Texture: StringProperty()
    Normal0InnerTexture: StringProperty()
    RefractionMask0: StringProperty()
    Normal1_Texture: StringProperty()
    Specular1_Texture: StringProperty()
    Colorchip0_Texture: StringProperty()
    MultiMask1_Texture: StringProperty()
    MultiMask2_Texture: StringProperty()
    NOTO0_Texture: StringProperty()
    Pattern0_Texture: StringProperty()
    AOTO0_Texture: StringProperty()
    Texture0: StringProperty()
    Occlusion1_Texture: StringProperty()
    DamageMask0_MMMM_Texture: StringProperty()
    Emissive0_Texture: StringProperty()
    DamageMask0_MMM_Texture: StringProperty()
    MultiMask_Texture_Lip: StringProperty()
    MRS_Texture_Hair: StringProperty()
    MRS_Texture_Makeup: StringProperty()
    MRS_Texture_Manicure: StringProperty()
    MultiMask_Texture_Manicure: StringProperty()
    MultiMask_Texture_wrinkle: StringProperty()
    Normal_Texture_wrinkle: StringProperty()
    Texture2DArray_Color_Makeup: StringProperty()
    Texture2DArray_Color_Scar: StringProperty()
    Texture2DArray_Color_WhiteEyebrow: StringProperty()
    Texture2DArray_Eyebrow_Shadow: StringProperty()
    Texture2DArray_Mask_WhiteHair: StringProperty()
    Texture2DArray_Maske_Hair: StringProperty()
    Texture2DArray_MultiMask_Eyebrow: StringProperty()
    Texture2DArray_MultiMask_Stubble: StringProperty()
    Texture2DArray_Normal_Eyebrow: StringProperty()
    Texture2DArray_Normal_Hair: StringProperty()
    Texture2DArray_Normal_Scar: StringProperty()
    Texture2DArray_Normal_Stubble: StringProperty()
    Texture2DArray_Occlusion_Glove: StringProperty()
    Texture2DArray_Occlusion_Hair: StringProperty()
    Texture2DArray_Occlusion_Pants: StringProperty()
    Texture2DArray_Occlusion_Shoes: StringProperty()
    Texture2DArray_Occlusion_Tops: StringProperty()
    Texture2DArray_Occlusion_Wear: StringProperty()
    Texture2DArray_Tattoo_Back: StringProperty()
    Texture2DArray_Tattoo_Front: StringProperty()
    Texture2DArray_Tattoo_Left: StringProperty()
    Texture2DArray_Tattoo_Right: StringProperty()
    MultiMask_Texture1: StringProperty()
    MultiMask_Texture10: StringProperty()
    MultiMask_Texture11: StringProperty()
    MultiMask_Texture12: StringProperty()
    MultiMask_Texture13: StringProperty()
    MultiMask_Texture14: StringProperty()
    MultiMask_Texture15: StringProperty()
    MultiMask_Texture16: StringProperty()
    MultiMask_Texture2: StringProperty()
    MultiMask_Texture3: StringProperty()
    MultiMask_Texture4: StringProperty()
    MultiMask_Texture5: StringProperty()
    MultiMask_Texture6: StringProperty()
    MultiMask_Texture7: StringProperty()
    MultiMask_Texture8: StringProperty()
    MultiMask_Texture9: StringProperty()
    Normal_Texture_Detail: StringProperty()
    MRS_Texture_Detail: StringProperty()
    MuliMask_Texture_Detail: StringProperty()
    Occlusion_Texture_Detail: StringProperty()
    Texture2DArray_Mark: StringProperty()
    BaseColorRedEye_Texture: StringProperty()
    EmissiveRedEye_Texture: StringProperty()
    MultiMask0_Texture0: StringProperty()
    MultiMaskRedEye_Texture: StringProperty()
    Transparency0_Texture: StringProperty()
    BaseColor2_Texture: StringProperty()
    FadingMask_Texture: StringProperty()
    Fazz0_Texture: StringProperty()
    FazzMask0_Texture: StringProperty()
    LayeredMask0_BaseColorAlphaTextures: StringProperty()
    LayeredMask0_MRSTextures: StringProperty()
    MRS1_Texture: StringProperty()
    MRS2_Texture0: StringProperty()
    MultiMask4_Texture: StringProperty()
    Normal1_Texture0: StringProperty()
    Normal2_Texture: StringProperty()
    Texture2DArray0: StringProperty()
    LayeredMask0_BaseColorAlphaTextures0: StringProperty()
    LayeredMask0_MRSTextures0: StringProperty()
    LayeredMask0_NormalTextures0: StringProperty()
    MultiMask3_Texture0: StringProperty()
    MRO_Mix0Texture: StringProperty()
    Normal0Texture: StringProperty()
    LayeredMask0_MaskTextures: StringProperty()
    LayeredMask0_MROTextures: StringProperty()
    Transparency0Texture: StringProperty()
    EmissiveColor0Texture: StringProperty()
    b1_DamageColorTexture: StringProperty()
    b2_RustColor_Texture: StringProperty()
    b3_RustColorField_Texture: StringProperty()
    mm_bodycolor_mask: StringProperty()
    mrs1_DamageMRS_Texture: StringProperty()
    mrs2_RustMRS_Texture: StringProperty()
    n1_NormalDamage_Texture: StringProperty()
    n2_RustNormal_Texture: StringProperty()
    RippleTex: StringProperty()
    Emissive1_Texture: StringProperty()
    Emissive2_Texture: StringProperty()
    Emissive3_Texture: StringProperty()
    Occlusion0Texture: StringProperty()
    b1_MudColorTexture: StringProperty()
    b2_DustColor_Texture: StringProperty()
    DirtField_mask: StringProperty()
    DropsMask_Texture: StringProperty()
    DustColorMaskTex: StringProperty()
    MetalFlake_NormalAndMask_Texture: StringProperty()
    mrs1_MudMRS_Texture: StringProperty()
    mrs2_DustMRS_Texture: StringProperty()
    mrs3_Scratch1MRS_Texture: StringProperty()
    mrs4_Scratch2MRS_Texture: StringProperty()
    MudNormalTex: StringProperty()
    Scratch1ColorTexture: StringProperty()
    Scratch1NormalTex: StringProperty()
    Scratch2ColorMaskTex: StringProperty()
    Scratch2ColorTexture: StringProperty()
    Scratch2NormalTex: StringProperty()
    ScratchFieldTexture: StringProperty()
    SprinklesMask_Texture: StringProperty()
    SprinklesNormal_Texture: StringProperty()
    Sticker1_Texture: StringProperty()
    Sticker2_Texture: StringProperty()
    Sticker3_Texture: StringProperty()
    Sticker4_Texture: StringProperty()
    Sticker5_Texture: StringProperty()
    GlassSpec_Texture: StringProperty()
    wipersMask_Texture: StringProperty()
    BackBufferCopy: StringProperty()
    DustTile_Texture: StringProperty()
    sprinklesMask_Texture: StringProperty()
    sprinklesNormal_Texture: StringProperty()
    DetailMask0_Texture: StringProperty()
    MultiMask3_Texture: StringProperty()


class MaterialSettings(PropertyGroup):
    def update_preset(self, context):
        match = None

        for prop in self.property_collection:
            if prop.material_id == self.preset:
                match = prop
                break

        if match is None:
            new_property_collection = self.property_collection.add()
            new_property_collection.material_id = self.preset
            for prop in material_properties[self.preset]:
                new_property = new_property_collection.property_collection.add()
                new_property.property_name = prop.property_name
                new_property.is_relevant = prop.is_relevant
                new_property.importance = prop.importance
                new_property.property_type = prop.property_type
                setattr(new_property_collection, prop.property_name, prop.default_value)

    preset: EnumProperty(
        items=material_enum,
        name='Material',
        description='Choose a material to apply to this mesh',
        default='NONE',
        options={'ANIMATABLE'},
        update=update_preset,
        get=None,
        set=None
    )

    show_advanced: BoolProperty(name='Show Advanced Options', default=False)
    property_collection: CollectionProperty(type=FlagrumMaterialPropertyCollection)
