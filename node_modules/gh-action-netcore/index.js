const exec = require('@actions/exec');
const glob = require('@actions/glob');

class GHActionNetCore {

    dirname = "";
    
    constructor(dirname){
        this.dirname = dirname;
    }

    async start() {
        const globber = await glob.create(this.dirname + '/**/*.csproj', { followSymbolicLinks: false })
        const files = await globber.glob()

        const projectPath = files[0];

        let args = ['run', '-c', 'Release', '--project', projectPath, '--'];

        var env = process.env;

        Object.keys(env).forEach(function (envKey) {
            if (envKey.startsWith("INPUT_")) {
                let key = `-${envKey.replace("INPUT_", "").toLowerCase()}`;
                args.push(key);
                args.push(env[envKey]);
            }
        });

        await exec.exec('dotnet', args);
    }
}


module.exports = GHActionNetCore