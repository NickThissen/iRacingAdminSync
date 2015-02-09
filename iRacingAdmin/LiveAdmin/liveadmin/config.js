var updateFrequency = 5;
var sessionsDir = 'sessions';

var columns = [  
  ['AdminShortName', 'Official', 60],
    ['Decided', 'Decided', 50],
  [ 'ProtestingTeamId', 'Protesting team #', 60],
  ['OffendingTeamId', 'Offending team #', 60],
  [ 'InvestigatedDriversDisplay', 'Driver(s)', 120 ],
  [ 'TimeGMT', 'Time (GMT)', 60, convertTime],
  [ 'Lap', 'Lap',50 ],
  [ 'Turn', 'Turn',50 ],
  [ 'RuleId', 'Rule #',50 ],
  [ 'Reason', 'Reason',150 ],
  [ 'Result', 'Outcome',150],
  [ 'ResultDecidedLap', 'Outcome lap' ,50],
  [ 'Served', 'Served',50 ]
];
