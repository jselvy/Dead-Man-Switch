﻿using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace DMS
{
    public class CompVehicleWeapon : ThingComp
    {
        public Pawn pawn;

        public float CurrentAngle => _currentAngle;
        public float TargetAngle
        {
            get
            {
    

                if (pawn.stances.curStance is Stance_Busy busy && busy.focusTarg.IsValid)
                {
                    Vector3 targetPos;
                    if (busy.focusTarg.HasThing)
                    {
                        targetPos = busy.focusTarg.Thing.DrawPos;
                    }
                    else
                    {
                        targetPos = busy.focusTarg.Cell.ToVector3Shifted();
                    }

                        
					return (targetPos - pawn.DrawPos).AngleFlat();
                    
                }

                return _turretFollowingAngle;
            }
        }


        private float _turretFollowingAngle = 0f;

        private float _turretAnglePerFrame = 0.1f;

        private float _currentAngle = 0f;
        private float _rotationSpeed = 0f;

        private Rot4 _lastRotation;

        public static readonly Dictionary<PawnRenderer, CompVehicleWeapon> cachedVehicles = new Dictionary<PawnRenderer, CompVehicleWeapon>();
        public static readonly Dictionary<CompVehicleWeapon, Pawn> cachedPawns = new Dictionary<CompVehicleWeapon, Pawn>();
        public static readonly Dictionary<Pawn, CompVehicleWeapon> cachedVehicldesPawns = new Dictionary<Pawn, CompVehicleWeapon>();

        public CompProperties_VehicleWeapon Props
        {
            get
            {
                return (CompProperties_VehicleWeapon)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pawn = parent as Pawn;

            if (pawn == null)
            {
                Log.Error("The CompVehicleWeapon is set on a non-pawn object.");
                return;
            }

            if (pawn.equipment.Primary == null && Props.defaultWeapon != null)
            {
                Thing weapon = ThingMaker.MakeThing(Props.defaultWeapon);
                pawn.equipment.AddEquipment((ThingWithComps)weapon);
            }

            CompVehicleWeapon.cachedVehicles.Add(((Pawn)parent).Drawer.renderer, this);
            CompVehicleWeapon.cachedPawns.Add(this, (Pawn)parent);
            CompVehicleWeapon.cachedVehicldesPawns.Add((Pawn)parent, this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            CompVehicleWeapon.cachedVehicles.Remove(((Pawn)parent).Drawer.renderer);
            CompVehicleWeapon.cachedPawns.Remove(this);
            CompVehicleWeapon.cachedVehicldesPawns.Remove((Pawn)parent);
        }


        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            CompVehicleWeapon.cachedVehicles.Remove(((Pawn)parent).Drawer.renderer);
            CompVehicleWeapon.cachedPawns.Remove(this);
            CompVehicleWeapon.cachedVehicldesPawns.Remove((Pawn)parent);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (pawn == null) return;

            if (Props.turretRotationFollowPawn)
            {
                if (pawn.Rotation == Rot4.South)
                    _turretFollowingAngle = 180;
                else if (pawn.Rotation == Rot4.North)
                    _turretFollowingAngle = 0;
                else if (pawn.Rotation == Rot4.East)
                    _turretFollowingAngle = 90;
                else if (pawn.Rotation == Rot4.West)
                    _turretFollowingAngle = 270;
            }
            else
            {
                _turretFollowingAngle += _turretAnglePerFrame;
            }

            if (_lastRotation != pawn.Rotation)
            {
                _lastRotation = pawn.Rotation;
                _currentAngle = _turretFollowingAngle;
            }

            _currentAngle = Mathf.SmoothDampAngle(_currentAngle, TargetAngle, ref _rotationSpeed, Props.rotationSmoothTime);

        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            _turretAnglePerFrame = Rand.Range(-0.5f, 0.5f);
        }
    }
    public class CompProperties_VehicleWeapon : CompProperties
    {
        public CompProperties_VehicleWeapon()
        {
            this.compClass = typeof(CompVehicleWeapon);
        }
        public Vector3 drawOffset = Vector3.zero;
        public bool turretRotationFollowPawn = false;
        public float rotationSmoothTime = 0.12f;
        public ThingDef defaultWeapon;
        public float drawSize = 2f;
    }
}
