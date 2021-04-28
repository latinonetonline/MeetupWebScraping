const GHActionNetCore = require('gh-action-netcore');

const action = new GHActionNetCore(__dirname);

action.start().catch(err => {
    console.error(err);
    console.error(err.stack);
    process.exit(-1);
})