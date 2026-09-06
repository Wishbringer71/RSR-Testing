#!/usr/bin/env python3
"""Find defensive/mitigation actions used inside methods that carry no danger gate."""
import os, re

MIT = ['RadiantAegisPvE','AddlePvE','FeintPvE','ManawardPvE','TemperaCoatPvE','TemperaGrassaPvE',
       'MagickBarrierPvE','ReprisalPvE','RampartPvE','SentinelPvE','GuardianPvE','ShadowWallPvE',
       'ShadowedVigilPvE','VengeancePvE','DamnationPvE','NebulaPvE','GreatNebulaPvE','BloodwhettingPvE',
       'RawIntuitionPvE','ThrillOfBattlePvE','DarkMissionaryPvE','HeartOfLightPvE','DivineVeilPvE',
       'PassageOfArmsPvE','ShieldSambaPvE','TacticianPvE','TroubadourPvE','TengentsuPvE','ThirdEyePvE',
       'RiddleOfEarthPvE','BloodbathPvE','SecondWindPvE','ArcaneCrestPvE','DismantlePvE','SelfSufficiencePvE']
# methods that run without any AutoStatus danger gate
UNGATED = ['GeneralAbility','AttackAbility','EmergencyAbility','GeneralGCD','AttackGCD']

sig = re.compile(r'(?:public|protected|private)[\w\s]*\b(?:override\s+)?bool\s+(\w+)\s*\([^)]*\)\s*\{')

def strip(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return src

def body(src, i):
    d=0; j=src.find('{', i)
    st=j
    while j < len(src):
        if src[j]=='{': d+=1
        elif src[j]=='}':
            d-=1
            if d==0: return src[st:j]
        j+=1
    return src[st:]

hits=[]
for dp,_,ns in os.walk('RotationSolver'):
    for n in ns:
        if not n.endswith('.cs'): continue
        p=os.path.join(dp,n)
        src=strip(open(p,encoding='utf-8').read())
        for m in sig.finditer(src):
            name=m.group(1)
            if name not in UNGATED: continue
            b=body(src, m.end()-1)
            for a in MIT:
                for mm in re.finditer(r'\b'+a+r'\.CanUse\(', b):
                    line = src[:m.end()].count('\n') + b[:mm.start()].count('\n') + 1
                    ctx = b[max(0,mm.start()-260):mm.start()+60].replace('\n',' ')
                    ctx = re.sub(r'\s+',' ',ctx)[-190:]
                    hits.append((p,line,name,a,ctx))
for h in hits:
    print(f'{h[0]}:{h[1]}  [{h[2]}] {h[3]}\n      ...{h[4]}')
print(f'\n{len(hits)} hits')
