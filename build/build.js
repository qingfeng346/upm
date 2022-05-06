const fs = require('fs')
const os = require('os')
const path = require('path')
const { spawn, exec, execSync } = require('child_process')
async function main() {
    let args = process.argv.splice(2)
    let cwd = process.cwd()
    let name = args[0]
    let version = args[1]
    console.log(`cwd:${cwd}  name:${name} version:${version}`)
    rmdir("./tmp")
    if (name == "sco") {
        await execSpawn("git", cwd, ["clone", "-b", `v${version}`, "https://github.com/qingfeng346/Scorpio-CSharp.git", "./tmp/sco"])
    } else if (name == "scov") {
        await execSpawn("git", cwd, ["clone", "-b", `v${version}`, "https://github.com/qingfeng346/ScorpioConversion.git", "./tmp/scov"])
    }
    try {
        let unityVersion = "2019.4.15f1"
        let unityPath = os.platform() == "win32" ? `D:/Program Files/${unityVersion}/Editor/Unity.exe` : `/Applications/${unityVersion}/Unity.app/Contents/MacOS/Unity`
        await execSpawn(unityPath, null, ["-batchmode", "-quit", "-projectPath", path.dirname(cwd), "-logFile", `${cwd}/unity.log`, "-executeMethod", "Command.Execute", "--args", "-name", name, "-version", version])
    } catch (e) {
        console.log(e)
    }
    rmdir("./tmp")
}
function execSpawn(command, cwd, args) {
    return new Promise((resolve) => {
        let sp = spawn(command, args, { cwd: cwd })
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
function rmdir(p) {
    if (!fs.existsSync(p)) { return; }
    let files = fs.readdirSync(p)
    files.forEach((file) => {
        let curPath = p + "/" + file
        if (fs.statSync(curPath).isDirectory()) {
            rmdir(curPath)
        } else {
            fs.unlinkSync(curPath)
        }
    })
    fs.rmdirSync(p)
}
main()