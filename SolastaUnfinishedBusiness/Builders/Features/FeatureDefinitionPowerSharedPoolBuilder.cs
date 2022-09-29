﻿using System;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.CustomDefinitions;

namespace SolastaUnfinishedBusiness.Builders.Features;

// ReSharper disable once ClassNeverInstantiated.Global
internal class FeatureDefinitionPowerSharedPoolBuilder : FeatureDefinitionPowerBuilder<
    FeatureDefinitionPowerSharedPool, FeatureDefinitionPowerSharedPoolBuilder>
{
    protected override void Initialise()
    {
        base.Initialise();

        // We set uses determination to fixed because the code handling updates needs that.
        Definition.usesDetermination = RuleDefinitions.UsesDetermination.Fixed;
    }

    internal override void Validate()
    {
        base.Validate();

        Preconditions.ArgumentIsNotNull(Definition.SharedPool,
            $"FeatureDefinitionPowerSharedPoolBuilder[{Definition.Name}].SharedPool is null.");
        Preconditions.AreEqual(Definition.UsesDetermination, RuleDefinitions.UsesDetermination.Fixed,
            $"FeatureDefinitionPowerSharedPoolBuilder[{Definition.Name}].UsesDetermination must be set to Fixed.");
    }

    public FeatureDefinitionPowerSharedPoolBuilder Configure(
        FeatureDefinitionPower poolPower,
        RuleDefinitions.RechargeRate recharge,
        RuleDefinitions.ActivationTime activationTime,
        int costPerUse,
        bool proficiencyBonusToAttack,
        bool abilityScoreBonusToAttack,
        string abilityScore,
        EffectDescription effectDescription,
        bool uniqueInstance)
    {
        Preconditions.ArgumentIsNotNull(poolPower,
            $"FeatureDefinitionPowerSharedPoolBuilder[{Definition.Name}] poolPower is null.");

        // Recharge rate probably shouldn't be in here, but for now leave it be because there is already usage outside of this mod
        Definition.rechargeRate = recharge;
        Definition.activationTime = activationTime;
        Definition.costPerUse = costPerUse;
        Definition.proficiencyBonusToAttack = proficiencyBonusToAttack;
        Definition.abilityScoreBonusToAttack = abilityScoreBonusToAttack;
        Definition.abilityScore = abilityScore;
        Definition.effectDescription = effectDescription;
        Definition.uniqueInstance = uniqueInstance;
        Definition.SharedPool = poolPower;

        return This();
    }
    
    public FeatureDefinitionPowerSharedPoolBuilder SetSharedPool(FeatureDefinitionPower poolPower)
    {
        Preconditions.ArgumentIsNotNull(poolPower,
            $"FeatureDefinitionPowerSharedPoolBuilder[{Definition.Name}] poolPower is null.");

        Definition.SharedPool = poolPower;

        //Recharge rate should match pool for tooltips to make sense
        Definition.rechargeRate = poolPower.RechargeRate;
        //Enforce usage determination as Fixed again, in case it was changed. Should we move it to some finalization method instead?
        Definition.usesDetermination = RuleDefinitions.UsesDetermination.Fixed;

        return this;
    }


    #region Constructors

    protected FeatureDefinitionPowerSharedPoolBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionPowerSharedPoolBuilder(string name, string definitionGuid) : base(name,
        definitionGuid)
    {
    }

    protected FeatureDefinitionPowerSharedPoolBuilder(FeatureDefinitionPowerSharedPool original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    protected FeatureDefinitionPowerSharedPoolBuilder(FeatureDefinitionPowerSharedPool original, string name,
        string definitionGuid) : base(original, name, definitionGuid)
    {
    }

    #endregion
}
