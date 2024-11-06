using System;
using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Component;
using YamlDotNet.RepresentationModel;

namespace Logic.Reader
{
    public class ComponentInfoReader
    {
        // 解析变换组件transform_comp的数据
        public static void ParseTransform(YamlMappingNode transformNode, TransformCompDef compDef, TransformComponent comp)
        {
            foreach (var tn in transformNode.Children)
            {
                var tkey = ((YamlScalarNode)tn.Key).Value;
                if (tkey == "pos")
                {
                    if (compDef != null)
                    {
                        compDef.Pos = ParsePos((YamlMappingNode)tn.Value);
                    }
                    if (comp != null)
                    {
                        comp.Pos = ParsePos((YamlMappingNode)tn.Value);
                    }
                }
                else if (tkey == "rotation")
                {
                    var degree = short.Parse(tn.Value.ToString());
                    if (degree != 0)
                    {
                        if (compDef != null)
                        {
                            compDef.Rotation = new Angle(degree, 0);
                        }
                        if (comp != null)
                        {
                            comp.Rotation = new Angle(degree, 0);
                        }
                    }
                }
                else if (tkey == "orientation_rotation_diff")
                {
                    var degree = short.Parse(tn.Value.ToString());
                    if (degree != 0)
                    {
                        if (compDef != null)
                        {
                            compDef.OrientationRotationDiff = new Angle(degree, 0);
                        }
                        if (comp != null)
                        {
                            comp.OrientationRotationDiff = new Angle(degree, 0);
                        }
                    }
                }
            }
        }

        // 解析运动组件movement_comp的数据
        public static void ParseMovement(YamlMappingNode movementNode, ref MovementCompDef compDef, bool createNew = false)
        {
            foreach (var n in movementNode.Children)
            {
                MovementType moveType = MovementType.None;
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "move_type")
                {
                    moveType = (MovementType)int.Parse(n.Value.ToString());
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    if (compDef != null)
                    {
                        compDef.MoveType = moveType;
                    }
                }
                else if (key == "move_on_awake")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    compDef.MoveOnAwake = bool.Parse(n.Value.ToString());
                }
                else if (key == "speed")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    compDef.Speed = int.Parse(n.Value.ToString());
                }
                else if (key == "rotation_speed")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    compDef.RotationSpeed = int.Parse(n.Value.ToString());
                }
                else if (key == "center")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    var circularMovementCompDef = (CircularMovementCompDef)compDef;
                    circularMovementCompDef.Center = ParsePos((YamlMappingNode)n.Value);
                }
                else if (key == "radius")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    var circularMovementCompDef = (CircularMovementCompDef)compDef;
                    circularMovementCompDef.Radius = int.Parse(n.Value.ToString());
                }
                else if (key == "angular_velocity")
                {
                    if (createNew)
                    {
                        compDef ??= CreateMovementCompDef(moveType);
                    }
                    var circularMovementCompDef = (CircularMovementCompDef)compDef;
                    circularMovementCompDef.AngularVelocity = int.Parse(n.Value.ToString());
                }
            }
        }

        static MovementCompDef CreateMovementCompDef(MovementType moveType)
        {
            if (moveType == MovementType.None || moveType == MovementType.Linear)
            {
                return new MovementCompDef();
            }
            else if (moveType == MovementType.Circular)
            {
                return new CircularMovementCompDef();
            }
            else if (moveType == MovementType.Elliptical)
            {
                return new EllipticalMovementCompDef();
            }
            else if (moveType == MovementType.Spiral)
            {
                return new SpiralMovementCompDef();
            }
            return null;
        }

        // 解析CampComponent的定义CampCompDef
        public static void ParseCamp(YamlMappingNode campNode, CampCompDef compDef, CampComponent comp)
        {
            foreach (var n in campNode.Children)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "type")
                {
                    if (compDef != null)
                    {
                        compDef.CampType = (CampType)int.Parse(n.Value.ToString());
                    }
                    if (comp != null)
                    {
                        comp.CampType = (CampType)int.Parse(n.Value.ToString());
                    }
                }
            }
        }

        // 解析TagComponent的定义TagCompDef
        public static void ParseTag(YamlMappingNode campNode, TagCompDef compDef, TagComponent comp)
        {
            foreach (var n in campNode.Children)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "name")
                {
                    if (compDef != null)
                    {
                        compDef.Name = n.Value.ToString();
                    }
                    if (comp != null)
                    {
                        comp.Name = n.Value.ToString();
                    }
                }
            }
        }

        public static void ParseShooting(YamlMappingNode shootingNode, ShootingCompDef compDef)
        {
            foreach (var n in shootingNode)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "pos")
                {
                    compDef.Pos = ParsePos((YamlMappingNode)n.Value);
                }
                else if (key == "pos_list")
                {
                    compDef.PosList = ParsePosList((YamlSequenceNode)n.Value);
                }
                else if (key == "projectile")
                {
                    compDef.Projectile = int.Parse(n.Value.ToString());
                }
                else if (key == "range")
                {
                    compDef.Range = int.Parse(n.Value.ToString());
                }
                else if (key == "interval")
                {
                    compDef.Interval = int.Parse(n.Value.ToString());
                }
            }
        }

        public static void ParseLifeTime(YamlMappingNode lifeNode, LifeTimeCompDef compDef)
        {
            foreach (var n in lifeNode)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "time_seconds")
                {
                    compDef.Seconds = int.Parse(n.Value.ToString());
                }
            }
        }

        public static ProjectileCompDef ParseProjectile(YamlMappingNode projectileNode)
        {
            var typeKey = new YamlScalarNode("type");
            if (!projectileNode.Children.ContainsKey(typeKey)) return null;
            var typeValue = projectileNode[typeKey];
            if (typeValue == null) return null;

            ProjectileCompDef compDef = null;
            var v = typeValue.ToString();
            if (v == "bullet")
            {
                compDef = new BulletCompDef
                {
                    PType = ProjectileType.Bullet
                };
            }
            else if (v == "missile")
            {
                compDef = new MissileCompDef
                {
                    PType = ProjectileType.Missile
                };
            }
            else if (v == "bomb")
            {
                compDef = new BombCompDef
                {
                    PType = ProjectileType.Bomb
                };
            }
            else if (v == "plasma")
            {
                compDef = new PlasmaCompDef
                {
                    PType = ProjectileType.Plasma
                };
            }
            else if (v == "beam")
            {
                compDef = new BeamCompDef
                {
                    PType = ProjectileType.Beam
                };
            }
            else if (v == "shockwave")
            {
                compDef = new ShockwaveCompDef
                {
                    PType = ProjectileType.Shockwave
                };
            }
            foreach (var n in projectileNode)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "range")
                {
                    compDef.Range = int.Parse(n.Value.ToString());
                }
                else if (key == "duration")
                {
                    if (compDef is BeamCompDef beamCompDef)
                    {
                        beamCompDef.Duration = int.Parse(n.Value.ToString());
                    }
                    else
                    {
                        throw new Exception("ProjectileCompDef convert to BeamCompDef failed");
                    }
                }
                else if (key == "damage")
                {
                    compDef.Damage = int.Parse(n.Value.ToString());
                }
                else if (key == "dps")
                {
                    if (compDef is BeamCompDef beamCompDef)
                    {
                        beamCompDef.DPS = int.Parse(n.Value.ToString());
                    }
                }
            }
            return compDef;
        }

        public static void ParseSearch(YamlMappingNode projectileNode, SearchCompDef compDef)
        {
            foreach (var n in projectileNode)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "radius")
                {
                    compDef.Radius = int.Parse(n.Value.ToString());
                }
                else if (key == "target_relation")
                {
                    compDef.TargetRelation = (CampRelation)int.Parse(n.Value.ToString());
                }
                else if (key == "intervals")
                {
                    compDef.Intervals = int.Parse(n.Value.ToString());
                }
            }
        }

        public static void ParseBehaviour(YamlMappingNode behaviourNode, BehaviourCompDef compDef)
        {
            foreach (var n in behaviourNode)
            {
                //string key = ((YamlScalarNode)n.Key).Value;
            }
        }

        public static void ParseCollider(YamlMappingNode colliderNode, ColliderCompDef compDef, ColliderComponent comp)
        {
            string shapeValue = "";
            int width = 0, height = 0;
            int radius = 0;
            int length = 0;
            int rotation = 0;
            bool isRigidbody = false;
            foreach (var n in colliderNode)
            {
                string key = ((YamlScalarNode)n.Key).Value;
                if (key == "shape")
                {
                    shapeValue = n.Value.ToString();
                }
                else if (key == "width")
                {
                    width = int.Parse(n.Value.ToString());
                }
                else if (key == "height")
                {
                    height = int.Parse(n.Value.ToString());
                }
                else if (key == "radius")
                {
                    radius = int.Parse(n.Value.ToString());
                }
                else if (key == "length")
                {
                    length = int.Parse(n.Value.ToString());
                }
                else if (key == "rotation")
                {
                    rotation = int.Parse(n.Value.ToString());
                }
                else if (key == "is_rigidbody")
                {
                    isRigidbody = bool.Parse(n.Value.ToString());
                }
            }

            if (shapeValue == "rect")
            {
                if (compDef != null)
                    compDef.Shape = new Shape(ShapeType.Rect, width, height);
                if (comp != null)
                    comp.Shape = new Shape(ShapeType.Rect, width, height);
            }
            else if (shapeValue == "circle")
            {
                if (compDef != null)
                    compDef.Shape = new Shape(ShapeType.Circle, radius);
                if (comp != null)
                    comp.Shape = new Shape(ShapeType.Circle, radius);
            }
            else if (shapeValue == "segment" || shapeValue == "line")
            {
                if (compDef != null)
                    compDef.Shape = new Shape(ShapeType.Segment, length, rotation);
                if (comp != null)
                    comp.Shape = new Shape(ShapeType.Segment, length, rotation);
            }
            if (compDef != null)
                compDef.IsRigidBody = isRigidbody;
            if (comp != null)
                comp.IsRigidBody = isRigidbody;
        }

        public static void ParseChar(YamlMappingNode charNode, CharacterCompDef compDef, CharacterComponent comp)
        {
            foreach (var n in charNode)
            {
                var key = ((YamlScalarNode)n.Key).Value;
                if (key == "max_hp")
                {
                    int maxHp = int.Parse(n.Value.ToString());
                    if (compDef != null)
                    {
                        compDef.MaxHp = maxHp;
                    }
                    if (comp != null)
                    {
                        comp.MaxHp = maxHp;
                    }
                }
                else if (key == "max_level")
                {
                    int maxLevel = int.Parse(n.Value.ToString());
                    if (compDef != null)
                    {
                        compDef.MaxLevel = maxLevel;
                    }
                    if (comp != null)
                    {
                        comp.MaxLevel = maxLevel;
                    }
                }
            }
        }

        public static Position ParsePos(YamlMappingNode n)
        {
            int x = 0, y = 0;
            foreach (var nn in n.Children)
            {
                string keyPos = ((YamlScalarNode)nn.Key).Value;
                if (keyPos == "x")
                {
                    x = int.Parse(nn.Value.ToString());
                }
                else if (keyPos == "y")
                {
                    y = int.Parse(nn.Value.ToString());
                }
            }
            return new Position(x, y);
        }

        static Position[] ParsePosList(YamlSequenceNode posNodeList)
        {
            var posList = new Position[posNodeList.Children.Count];
            for (int i = 0; i < posNodeList.Children.Count; i++)
            {
                var node = (YamlMappingNode)posNodeList.Children[i];
                posList[i] = ParsePos(node);
            }
            return posList;
        }
    }
}