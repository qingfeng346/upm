const { spawn } = require('child_process')
async function main() {
    let args = process.argv.splice(2)
    let version = args[0]
    let unityPath = `D:/Program Files/2018.4.36f1/Editor/Unity.exe`
    await exec("git", ["clone", "-b", `v${version}`, "https://gitee.com/qingfeng346/Scorpio-CSharp.git", "./temp"])
    await exec(unityPath, ["-batchmode", "-quit", "-projectPath", "./upm/", "-logFile", "./unity.log", "-executeMethod", "Command.Execute", "--args", "-version", version])
}
function exec(command, args) {
    return new Promise((resolve) => {
        let sp = spawn(command, args, { cwd: process.cwd() })
        let stdout = ""
        let stderr = ""
        sp.stdout.on('data', (data) => {
            let str = data.toString()
            stdout += str
            console.log(str)
        });
        sp.stderr.on('data', (data) => {
            let str = data.toString()
            stderr += str
            console.log(str)
        });
        sp.on("close", (code) => {
            resolve({ code: code, stdout: stdout, stderr: stderr })
        })
    })
}
main()