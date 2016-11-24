﻿using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _Pawn_RelationsTracker : Pawn_RelationsTracker
    {

        internal static LifeStageDef        MechanoidFullyFormed = DefDatabase<LifeStageDef>.GetNamed( "MechanoidFullyFormed" );

        internal static FieldInfo           _pawn;

        public                              _Pawn_RelationsTracker( Pawn pawn ) : base( pawn )
        {
        }

        static                              _Pawn_RelationsTracker()
        {
            _pawn = typeof( Pawn_RelationsTracker ).GetField( "pawn", Controller.Data.UniversalBindingFlags );
            if( _pawn == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'pawn' in 'Pawn_RelationsTracker'",
                    "Detour.Pawn_RelationsTracker" );
            }
        }

        internal Pawn                       GetPawn()
        {
            return (Pawn)_pawn.GetValue( this );
        }

        internal static bool                PawnsAreValidMatches( Pawn pawn1, Pawn pawn2 )
        {
            /*
            Log.Message(
                string.Format(
                    "PawnsAreValidMatches( {0}, {1} )\n\t{0} is human like: {2}\n\t{1} is human like: {3}\n\t{0} gender: {4}\n\t{1} gender: {5}\n\t{0} is gay: {6}\n\t{1} is gay: {7}\n\t{0} PawnKindDef: {8}\n\t{1} PawnKindDef: {9}",
                    pawn1.LabelShort,
                    pawn2.LabelShort,
                    pawn1.RaceProps.Humanlike,
                    pawn2.RaceProps.Humanlike,
                    pawn1.gender,
                    pawn2.gender,
                    pawn1.story.traits.HasTrait(TraitDefOf.Gay),
                    pawn2.story.traits.HasTrait(TraitDefOf.Gay),
                    pawn1.kindDef.defName,
                    pawn2.kindDef.defName
                ) );
            */
            if(
                ( !pawn1.RaceProps.Humanlike )||
                (
                    ( pawn1.RaceProps.Humanlike )&&
                    ( !pawn2.RaceProps.Humanlike )
                )||
                ( pawn1 == pawn2 )||
                ( pawn1.RaceProps.fleshType != FleshType.Normal )||
                ( pawn2.RaceProps.fleshType != FleshType.Normal )
                /*||
                (
                    ( !pawn1.RaceProps.lifeStageAges.NullOrEmpty() )&&
                    ( pawn1.RaceProps.lifeStageAges.Any( stage => stage.def == MechanoidFullyFormed ) )
                )||
                (
                    ( !pawn2.RaceProps.lifeStageAges.NullOrEmpty() )&&
                    ( pawn2.RaceProps.lifeStageAges.Any( stage => stage.def == MechanoidFullyFormed ) )
                )*/
            )
            {
                return false;
            }
            return true;
        }

        [DetourMember]
        internal float                      _CompatibilityWith( Pawn otherPawn )
        {
            var pawn = this.GetPawn();
            if( !PawnsAreValidMatches( pawn, otherPawn ) )
            {
                return 0f;
            }
            return Mathf.Clamp( GenMath.LerpDouble( 0.0f, 20f, 0.45f, -0.45f, Mathf.Abs( pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat ) ), -0.45f, 0.45f) + this.ConstantPerPawnsPairCompatibilityOffset( otherPawn.thingIDNumber );
        }

        [DetourMember]
        internal float                      _AttractionTo( Pawn otherPawn )
        {
            var pawn = this.GetPawn();

            if( !PawnsAreValidMatches( pawn, otherPawn ) )
            {
                return 0f;
            }
            float num = 1f;
            float num2 = 1f;
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if( pawn.gender == Gender.Male )
            {
                if(
                    ( pawn.RaceProps.Humanlike )&&
                    ( pawn.story.traits.HasTrait( TraitDefOf.Gay ) )
                )
                {
                    if( otherPawn.gender == Gender.Female )
                    {
                        return 0f;
                    }
                }
                else if( otherPawn.gender == Gender.Male )
                {
                    return 0f;
                }
                num2 = GenMath.FlatHill( 16f, 20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 15f, ageBiologicalYearsFloat2 );
            }
            else if( pawn.gender == Gender.Female )
            {
                if(
                    ( pawn.RaceProps.Humanlike )&&
                    ( pawn.story.traits.HasTrait( TraitDefOf.Gay ) )
                )
                {
                    if( otherPawn.gender == Gender.Male )
                    {
                        return 0f;
                    }
                }
                else if( otherPawn.gender == Gender.Female )
                {
                    num = 0.15f;
                }
                if( ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f )
                {
                    return 0f;
                }
                if( ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f )
                {
                    num2 = Mathf.InverseLerp( ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2 ) * 0.2f;
                }
                else
                {
                    num2 = GenMath.FlatHill( 0.2f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 40f, 0.1f, ageBiologicalYearsFloat2 );
                }
            }
            float num3 = 1f;
            num3 *= Mathf.Lerp( 0.2f, 1f, otherPawn.health.capacities.GetEfficiency( PawnCapacityDefOf.Talking ) );
            num3 *= Mathf.Lerp( 0.2f, 1f, otherPawn.health.capacities.GetEfficiency( PawnCapacityDefOf.Manipulation ) );
            num3 *= Mathf.Lerp( 0.2f, 1f, otherPawn.health.capacities.GetEfficiency( PawnCapacityDefOf.Moving ) );
            float num4 = 1f;
            foreach( PawnRelationDef current in pawn.GetRelations( otherPawn ) )
            {
                num4 *= current.attractionFactor;
            }
            int num5 = 0;
            if( otherPawn.RaceProps.Humanlike )
            {
                num5 = otherPawn.story.traits.DegreeOfTrait( TraitDefOf.Beauty );
            }
            float num6 = 1f;
            if( num5 < 0 )
            {
                num6 = 0.3f;
            }
            else if( num5 > 0 )
            {
                num6 = 2.3f;
            }
            float num7 = Mathf.InverseLerp( 15f, 18f, ageBiologicalYearsFloat );
            float num8 = Mathf.InverseLerp( 15f, 18f, ageBiologicalYearsFloat2 );
            return num * num2 * num3 * num4 * num7 * num8 * num6;
        }

    }

}
